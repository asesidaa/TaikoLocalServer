using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Contracts.AdminApi;
using TaikoLocalServer.Contracts.AdminApi.Authorization;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Avatars;

/// <summary>
/// <see cref="IAvatarRenderer"/> with a headless Edge or Chrome already on the machine (nothing is downloaded). It loads
/// the WebUI's 3D viewer (don-render.html) from this server, asks it for a snapshot, and stores it in UserAvatars
/// under the shared <see cref="AvatarKeys.Key"/>. One render at a time; the browser opens on demand and closes after a
/// few idle minutes, so an idle server pays nothing.
/// </summary>
public sealed class HeadlessAvatarRenderer(
    IServiceScopeFactory scopes,
    IServer server,
    IOptions<ServerSettings> settings,
    ILogger<HeadlessAvatarRenderer> logger) : BackgroundService, IAvatarRenderer
{
    private static readonly TimeSpan IdleClose = TimeSpan.FromMinutes(5);
    // The browser's profile, one per server (by its program and config folders, so servers sharing a machine don't
    // end each other's browsers): KillWithServer.EndOrphans finds a killed run's browser by it.
    private static readonly string ProfileDir = Path.Combine(Path.GetTempPath(), "taiko-avatar-renderer-" + Convert.ToHexString(
        SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(AppContext.BaseDirectory + "|" + Environment.CurrentDirectory)))[..12].ToLowerInvariant());
    private static readonly TimeSpan RenderTimeout = TimeSpan.FromSeconds(30);
    // A key that failed isn't tried again for this long (each avatar request that misses would queue it).
    private static readonly TimeSpan RetryAfter = TimeSpan.FromMinutes(10);
    // A player is rendered once they've left their costume alone this long: clicking through parts on Profile (each
    // autosave queues them) gives one render, and their own browser's upload usually lands first, which skips it.
    private static readonly TimeSpan Settle = TimeSpan.FromSeconds(3);

    private readonly Channel<uint> queue = Channel.CreateUnbounded<uint>();
    // Queued players, with when they were last queued.
    private readonly ConcurrentDictionary<uint, DateTime> queued = new();
    private readonly ConcurrentDictionary<string, DateTime> failedKeys = new();
    // The renderer's own bearer token: asset reads only (see ApiTokenAuthenticationHandler). New on every start.
    private readonly string token = ApiScopes.Prefix + "render_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24));

    private bool enabled;
    // Stills are drawn only while cards use them (ServerOptions.Cards3D off).
    private bool stills;
    private string? browserPath;
    private IBrowser? browser;
    private IPage? page;
    private int rendered, failed;
    private string? lastError;
    private DateTime? lastRenderedAt;

    public AvatarRendererStatus Status =>
        new(enabled, browserPath, browser is { IsClosed: false }, queued.Count, rendered, failed, lastError, lastRenderedAt, !stills);

    public bool IsRendererToken(string candidate) =>
        candidate.Length == token.Length && CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.ASCII.GetBytes(candidate), System.Text.Encoding.ASCII.GetBytes(token));

    public void Enqueue(uint baid, bool changed = false)
    {
        if (!enabled || browserPath is null) return;
        var added = false;
        // The time of the last change: a request keeps it (or, with no change pending, needs no wait at all).
        queued.AddOrUpdate(baid, _ => { added = true; return changed ? DateTime.UtcNow : DateTime.MinValue; },
            (_, last) => changed ? DateTime.UtcNow : last);
        if (added) queue.Writer.TryWrite(baid);
    }

    public async Task SetEnabledAsync(bool on, CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        var row = await db.ServerOptions.FindAsync([1], cancellationToken) ?? db.ServerOptions.Add(new ServerOptions()).Entity;
        row.RenderAvatars = on;
        await db.SaveChangesAsync(cancellationToken);
        enabled = on;
        if (!on) await CloseBrowserAsync();
    }

    public async Task SetCards3DAsync(bool on, CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        var row = await db.ServerOptions.FindAsync([1], cancellationToken) ?? db.ServerOptions.Add(new ServerOptions()).Entity;
        row.Cards3D = on;
        await db.SaveChangesAsync(cancellationToken);
        stills = !on;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        browserPath = FindBrowser();
        using (var scope = scopes.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
            var options = await db.ServerOptions.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 1, stoppingToken);
            enabled = options?.RenderAvatars ?? false;
            stills = !(options?.Cards3D ?? true);
        }
        if (browserPath is null)
        {
            lastError = "No Microsoft Edge or Google Chrome found. Install one, or set ServerSettings.AvatarBrowserPath.";
            logger.LogInformation("Avatar renderer: {Problem}", lastError);
        }
        // Give the server a moment to start listening before the first page load.
        try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); } catch (OperationCanceledException) { return; }
        // On demand only: a player is drawn when their avatar or still is shown without an up-to-date image, or their
        // costume is saved. Nothing goes over every player, so a server with thousands of them (or one just migrated)
        // never has hours of renders waiting; only the players people look at cost anything.

        while (!stoppingToken.IsCancellationRequested)
        {
            uint baid;
            using (var idle = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
            {
                idle.CancelAfter(IdleClose);
                try
                {
                    baid = await queue.Reader.ReadAsync(idle.Token);
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    await CloseBrowserAsync();
                    continue;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            try
            {
                // Each change while waiting pushes the render back (see Settle).
                while (queued.TryGetValue(baid, out var changed) && Settle - (DateTime.UtcNow - changed) is { Ticks: > 0 } wait)
                    await Task.Delay(wait, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            queued.TryRemove(baid, out _);
            if (!enabled || browserPath is null) continue;
            try
            {
                await RenderAsync(baid, stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException || !stoppingToken.IsCancellationRequested)
            {
                failed++;
                lastError = e.Message;
                logger.LogWarning("Avatar renderer: player {Baid} failed: {Message}", baid, e.Message);
                // A broken browser is started again for the next job.
                await CloseBrowserAsync();
            }
        }
        await CloseBrowserAsync();
    }

    private async Task RenderAsync(uint baid, CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        // Nijiiro's costume (the 3D models are Nijiiro's); a player who has only played older games has none.
        var user = await db.UserSaveDataNijiiro.AsNoTracking().FirstOrDefaultAsync(u => u.Baid == baid, cancellationToken);
        if (user is null) return;
        var profile = await db.UserWebProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Baid == baid, cancellationToken);
        var animation = profile?.CardAnimation ?? default;
        var key = AvatarKeys.Key(user.CurrentKigurumi, user.CurrentHead, user.CurrentBody, user.CurrentFace,
            user.ColorBody, user.ColorFace, user.ColorLimb, animation);
        var stillKey = AvatarKeys.StillKey(user.CurrentKigurumi, user.CurrentHead, user.CurrentBody, user.CurrentFace, user.CurrentPuchi,
            user.ColorBody, user.ColorFace, user.ColorLimb, animation);
        var stored = await db.UserAvatars.FindAsync([baid], cancellationToken);
        var storedStill = await db.UserStills.FindAsync([baid], cancellationToken);
        bool needAvatar = stored?.Key != key, needStill = stills && storedStill?.Key != stillKey;
        if (!needAvatar && !needStill) return;
        var failKey = $"{key}|{stillKey}";
        if (failedKeys.TryGetValue(failKey, out var at) && DateTime.UtcNow - at < RetryAfter) return;

        var p = await PageAsync();
        try
        {
            // With the puchi (the still shows it; the avatar hides it). A puchi whose files are missing would stop the
            // whole model loading, so that falls back to none.
            if (!await LoadAsync(p, user, animation, user.CurrentPuchi) && user.CurrentPuchi != 0 && !await LoadAsync(p, user, animation, 0))
                throw new InvalidOperationException("The 3D model didn't load.");
            if (needAvatar)
            {
                var (image, contentType) = Decode(await SnapshotOrReloadAsync(p, user, animation, AvatarFraming));
                if (image.Length > UserAvatar.MaxBytes) throw new InvalidOperationException("The avatar image is too big.");
                stored = await db.UserAvatars.FindAsync([baid], cancellationToken);
                if (stored is null) db.UserAvatars.Add(stored = new UserAvatar { Baid = baid });
                stored.Image = image;
                stored.ContentType = contentType;
                stored.Key = key;
            }
            if (needStill)
            {
                var (image, contentType) = Decode(await SnapshotOrReloadAsync(p, user, animation, StillFraming));
                if (image.Length > UserStill.MaxBytes) throw new InvalidOperationException("The still image is too big.");
                storedStill = await db.UserStills.FindAsync([baid], cancellationToken);
                if (storedStill is null) db.UserStills.Add(storedStill = new UserStill { Baid = baid });
                storedStill.Image = image;
                storedStill.ContentType = contentType;
                storedStill.Key = stillKey;
            }
            await db.SaveChangesAsync(cancellationToken);
            rendered++;
            lastRenderedAt = DateTime.UtcNow;
            failedKeys.TryRemove(failKey, out _);
        }
        catch
        {
            failedKeys[failKey] = DateTime.UtcNow;
            throw;
        }
    }

    // The avatar: as the WebUI's (ui.js avatarRun), face centred, no puchi, 256px. Bump AvatarKeys.Key's version to change it.
    // Every snapshot is drawn at twice its size and scaled down (SnapshotAsync), for smoother edges.
    private static readonly object AvatarFraming = new { zoom = 0.14, camY = 0.165, camX = 0.04, puchi = false, size = 256 };
    // The still: the whole figure with its puchi and some margin, 512px. Bump AvatarKeys.StillKey's version to change it.
    private static readonly object StillFraming = new { zoom = 0.26, camY = 0.15, camX = 0.02, puchi = true, size = 512 };

    // Opens the viewer on this costume; false when the model didn't load (e.g. a part's files are missing).
    private async Task<bool> LoadAsync(IPage p, UserSaveDataNijiiro user, CardAnimation animation, uint puchi)
    {
        var query = $"body={user.CurrentBody}&head={user.CurrentHead}&kigurumi={user.CurrentKigurumi}&face={user.CurrentFace}&puchi={puchi}" +
                    $"&bodyColor={user.ColorBody}&faceColor={user.ColorFace}&limbColor={user.ColorLimb}&anim={Uri.EscapeDataString(AvatarKeys.Clip(animation))}";
        await p.GoToAsync($"{BaseUrl()}don-render.html?{query}", new NavigationOptions { Timeout = (int)RenderTimeout.TotalMilliseconds });
        // The viewer's status turns {loading:false} once the model is on screen, or has an error (e.g. assets not extracted).
        await p.WaitForFunctionAsync("() => window.don && window.don.status.loading === false",
            new WaitForFunctionOptions { Timeout = (int)RenderTimeout.TotalMilliseconds });
        var error = await p.EvaluateExpressionAsync<string>("window.don.status.error || ''");
        if (error.Length > 0) logger.LogInformation("Avatar renderer: player {Baid}'s model didn't load with puchi {Puchi} ({Error})", user.Baid, puchi, error);
        return error.Length == 0;
    }

    // A snapshot that stays empty gets one more try on a freshly loaded page (the first snapshot after a load is the
    // dependable one).
    private async Task<string> SnapshotOrReloadAsync(IPage p, UserSaveDataNijiiro user, CardAnimation animation, object framing)
    {
        try
        {
            return await SnapshotAsync(p, framing);
        }
        catch (EvaluationFailedException)
        {
            if (!await LoadAsync(p, user, animation, user.CurrentPuchi) && !await LoadAsync(p, user, animation, 0))
                throw;
            return await SnapshotAsync(p, framing);
        }
    }

    // Right after loading (or after the last snapshot) the pose isn't applied yet and a snapshot can come back empty,
    // so let frames run and retry until it has pixels.
    private static Task<string> SnapshotAsync(IPage p, object framing) =>
        p.EvaluateFunctionAsync<string>("""
                async framing => {
                  const frame = () => new Promise(r => requestAnimationFrame(r));
                  const snap = () => {
                    const url = window.don.snapshot({ ...framing, size: framing.size * 2 });
                    if (!url) throw new Error("no snapshot");
                    return url;
                  };
                  const opaque = async url => {
                    const img = new Image(); img.src = url; await img.decode();
                    const c = new OffscreenCanvas(img.width, img.height), g = c.getContext("2d"); g.drawImage(img, 0, 0);
                    const d = g.getImageData(0, 0, img.width, img.height).data;
                    for (let i = 3; i < d.length; i += 4) if (d[i]) return true;
                    return false;
                  };
                  // Supersampling: the 2x snapshot scaled down with high-quality smoothing antialiases the outlines.
                  const shrink = async url => {
                    const img = new Image(); img.src = url; await img.decode();
                    const c = document.createElement("canvas"); c.width = c.height = framing.size;
                    const g = c.getContext("2d"); g.imageSmoothingQuality = "high";
                    g.drawImage(img, 0, 0, framing.size, framing.size);
                    return c.toDataURL("image/png");
                  };
                  const start = Date.now();
                  while (Date.now() - start < 8000) {
                    for (let i = 0; i < 10; i++) await frame();
                    const url = snap();
                    if (await opaque(url)) return await shrink(url);
                  }
                  throw new Error("the snapshot stayed empty");
                }
                """, framing);

    private static (byte[] Image, string ContentType) Decode(string dataUrl)
    {
        var comma = dataUrl.IndexOf(',');
        var head = comma > 0 ? dataUrl[..comma] : "";
        var contentType = head.Contains("image/webp") ? "image/webp" : head.Contains("image/png") ? "image/png"
            : throw new InvalidOperationException("The snapshot isn't an image.");
        return (Convert.FromBase64String(dataUrl[(comma + 1)..]), contentType);
    }

    private async Task<IPage> PageAsync()
    {
        if (page is { IsClosed: false } && browser is { IsClosed: false }) return page;
        await CloseBrowserAsync();
        // A server that's killed rather than shut down can't close the browser itself: on Windows the job ends it, on
        // Linux the next run does (by the profile folder).
        KillWithServer.Enable();
        KillWithServer.EndOrphans(ProfileDir);
        browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            ExecutablePath = browserPath,
            Headless = true,
            UserDataDir = ProfileDir,
            // WebGL without a GPU falls back to SwiftShader; the server's own certificate may be self-signed.
            // Chrome refuses to start as root (a Linux server bound to ports 80/443) unless sandboxing is off; it only
            // loads this server's own viewer.
            Args = ["--enable-unsafe-swiftshader", "--ignore-certificate-errors", "--no-first-run", "--mute-audio", "--disable-extensions",
                    .. OperatingSystem.IsLinux() && Environment.UserName == "root" ? ["--no-sandbox"] : Array.Empty<string>()],
        });
        page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 512, Height = 512 });
        // Before the viewer runs: its sign-in token (it reads authToken like the WebUI).
        await page.EvaluateExpressionOnNewDocumentAsync(
            $"localStorage.setItem(\"authToken\", {JsonSerializer.Serialize(JsonSerializer.Serialize(token))});");
        logger.LogInformation("Avatar renderer: started {Browser}", browserPath);
        return page;
    }

    private async Task CloseBrowserAsync()
    {
        var b = browser;
        browser = null;
        page = null;
        if (b is null) return;
        try { await b.CloseAsync(); } catch (Exception e) { logger.LogDebug(e, "Avatar renderer: closing the browser"); }
        b.Dispose();
    }

    // This server's own address, as the browser on the same machine reaches it (http preferred).
    private string BaseUrl()
    {
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses ?? [];
        var address = addresses.FirstOrDefault(a => a.StartsWith("http://")) ?? addresses.FirstOrDefault()
                      ?? throw new InvalidOperationException("The server has no address to load the 3D model from.");
        var uri = new UriBuilder(address.Replace("://*", "://localhost").Replace("://+", "://localhost").Replace("://[::]", "://localhost"));
        if (uri.Host is "0.0.0.0" or "localhost") uri.Host = "127.0.0.1";
        uri.Path = "/";
        return uri.Uri.ToString();
    }

    private string? FindBrowser()
    {
        if (!string.IsNullOrWhiteSpace(settings.Value.AvatarBrowserPath))
            return File.Exists(settings.Value.AvatarBrowserPath) ? settings.Value.AvatarBrowserPath : null;
        string[] candidates = OperatingSystem.IsWindows()
            ? [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe"),
            ]
            : ["/usr/bin/microsoft-edge", "/usr/bin/google-chrome", "/usr/bin/chromium", "/usr/bin/chromium-browser",
               "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge", "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"];
        return candidates.FirstOrDefault(File.Exists);
    }
}
