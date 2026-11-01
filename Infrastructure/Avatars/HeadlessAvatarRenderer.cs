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

namespace TaikoLocalServer.Infrastructure.Avatars;

/// <summary>
/// <see cref="IAvatarRenderer"/> with a headless Edge or Chrome already on the machine (nothing is downloaded). It loads
/// the WebUI's 3D embed from this server, asks it for the same snapshot the WebUI makes, and stores it in UserAvatars
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
    private static readonly TimeSpan RenderTimeout = TimeSpan.FromSeconds(30);
    // A key that failed isn't tried again for this long (each avatar request that misses would queue it).
    private static readonly TimeSpan RetryAfter = TimeSpan.FromMinutes(10);

    private readonly Channel<uint> queue = Channel.CreateUnbounded<uint>();
    private readonly ConcurrentDictionary<uint, byte> queued = new();
    private readonly ConcurrentDictionary<string, DateTime> failedKeys = new();
    // The renderer's own bearer token: asset reads only (see ApiTokenAuthenticationHandler). New on every start.
    private readonly string token = ApiScopes.Prefix + "render_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24));

    private bool enabled;
    private string? browserPath;
    private IBrowser? browser;
    private IPage? page;
    private int rendered, failed;
    private string? lastError;
    private DateTime? lastRenderedAt;

    public AvatarRendererStatus Status =>
        new(enabled, browserPath, browser is { IsClosed: false }, queued.Count, rendered, failed, lastError, lastRenderedAt);

    public bool IsRendererToken(string candidate) =>
        candidate.Length == token.Length && CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.ASCII.GetBytes(candidate), System.Text.Encoding.ASCII.GetBytes(token));

    public void Enqueue(uint baid)
    {
        if (enabled && browserPath is not null && queued.TryAdd(baid, 0)) queue.Writer.TryWrite(baid);
    }

    public async Task SetEnabledAsync(bool on, CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        var row = await db.ServerOptions.FindAsync([1], cancellationToken) ?? db.ServerOptions.Add(new ServerOptions()).Entity;
        row.RenderAvatars = on;
        await db.SaveChangesAsync(cancellationToken);
        enabled = on;
        if (on) await QueueStaleAsync(cancellationToken);
        else await CloseBrowserAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        browserPath = FindBrowser();
        using (var scope = scopes.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
            enabled = (await db.ServerOptions.AsNoTracking().FirstOrDefaultAsync(stoppingToken))?.RenderAvatars ?? true;
        }
        if (browserPath is null)
        {
            lastError = "No Microsoft Edge or Google Chrome found. Install one, or set ServerSettings.AvatarBrowserPath.";
            logger.LogInformation("Avatar renderer: {Problem}", lastError);
        }
        // Give the server a moment to start listening before the first page load.
        try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); } catch (OperationCanceledException) { return; }
        if (enabled) await QueueStaleAsync(stoppingToken);

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
        var user = await db.UserData.AsNoTracking().FirstOrDefaultAsync(u => u.Baid == baid, cancellationToken);
        if (user is null) return;
        var profile = await db.UserWebProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Baid == baid, cancellationToken);
        var animation = profile?.CardAnimation ?? default;
        var key = AvatarKeys.Key(user.CurrentKigurumi, user.CurrentHead, user.CurrentBody, user.CurrentFace,
            user.ColorBody, user.ColorFace, user.ColorLimb, animation);
        var stored = await db.UserAvatars.FindAsync([baid], cancellationToken);
        if (stored?.Key == key) return;
        if (failedKeys.TryGetValue(key, out var at) && DateTime.UtcNow - at < RetryAfter) return;

        var p = await PageAsync();
        var query = $"body={user.CurrentBody}&head={user.CurrentHead}&kigurumi={user.CurrentKigurumi}&face={user.CurrentFace}&puchi=0" +
                    $"&bodyColor={user.ColorBody}&faceColor={user.ColorFace}&limbColor={user.ColorLimb}&anim={Uri.EscapeDataString(AvatarKeys.Clip(animation))}";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RenderTimeout);
        try
        {
            await p.GoToAsync($"{BaseUrl()}don-embed/embed.html?{query}", new NavigationOptions { Timeout = (int)RenderTimeout.TotalMilliseconds });
            // The embed reports {loading:false} once the model is on screen, or an error (e.g. assets not extracted).
            await p.WaitForFunctionAsync("() => window.__don && window.__don.loading === false",
                new WaitForFunctionOptions { Timeout = (int)RenderTimeout.TotalMilliseconds });
            var error = await p.EvaluateExpressionAsync<string>("window.__don.error || ''");
            if (error.Length > 0) throw new InvalidOperationException($"The 3D model didn't load ({error}).");
            // The same framing as the WebUI's (ui.js avatarRun): face centred, no puchi, 256px. Right after loading the
            // pose isn't applied yet and the snapshot comes back empty, so let frames run and retry until it has pixels.
            var dataUrl = await p.EvaluateFunctionAsync<string>("""
                async () => {
                  const frame = () => new Promise(r => requestAnimationFrame(r));
                  const snap = () => new Promise((resolve, reject) => {
                    window.__snap = null;
                    postMessage({ type: "don-snapshot", zoom: 0.14, camY: 0.165, camX: 0.04, puchi: false, size: 256 }, location.origin);
                    const start = Date.now();
                    (function poll() {
                      if (window.__snap) resolve(window.__snap);
                      else if (Date.now() - start > 5000) reject(new Error("no snapshot"));
                      else setTimeout(poll, 50);
                    })();
                  });
                  const opaque = async url => {
                    const img = new Image(); img.src = url; await img.decode();
                    const c = new OffscreenCanvas(img.width, img.height), g = c.getContext("2d"); g.drawImage(img, 0, 0);
                    const d = g.getImageData(0, 0, img.width, img.height).data;
                    for (let i = 3; i < d.length; i += 4) if (d[i]) return true;
                    return false;
                  };
                  for (let attempt = 0; attempt < 20; attempt++) {
                    for (let i = 0; i < 5; i++) await frame();
                    const url = await snap();
                    if (await opaque(url)) return url;
                  }
                  throw new Error("the snapshot stayed empty");
                }
                """);
            var (image, contentType) = Decode(dataUrl);
            if (image.Length > UserAvatar.MaxBytes) throw new InvalidOperationException("The avatar image is too big.");

            stored = await db.UserAvatars.FindAsync([baid], cancellationToken);
            if (stored is null) db.UserAvatars.Add(stored = new UserAvatar { Baid = baid });
            stored.Image = image;
            stored.ContentType = contentType;
            stored.Key = key;
            await db.SaveChangesAsync(cancellationToken);
            rendered++;
            lastRenderedAt = DateTime.UtcNow;
            failedKeys.TryRemove(key, out _);
        }
        catch
        {
            failedKeys[key] = DateTime.UtcNow;
            throw;
        }
    }

    private static (byte[] Image, string ContentType) Decode(string dataUrl)
    {
        var comma = dataUrl.IndexOf(',');
        var head = comma > 0 ? dataUrl[..comma] : "";
        var contentType = head.Contains("image/webp") ? "image/webp" : head.Contains("image/png") ? "image/png"
            : throw new InvalidOperationException("The snapshot isn't an image.");
        return (Convert.FromBase64String(dataUrl[(comma + 1)..]), contentType);
    }

    // Everyone whose stored avatar doesn't show their current costume (a costume changed in game, a new player…).
    private async Task QueueStaleAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITaikoDbContext>();
        var keys = await db.UserAvatars.AsNoTracking().ToDictionaryAsync(a => a.Baid, a => a.Key, cancellationToken);
        var animations = await db.UserWebProfiles.AsNoTracking().ToDictionaryAsync(p => p.Baid, p => p.CardAnimation, cancellationToken);
        var users = await db.UserData.AsNoTracking()
            .Select(u => new { u.Baid, u.CurrentKigurumi, u.CurrentHead, u.CurrentBody, u.CurrentFace, u.ColorBody, u.ColorFace, u.ColorLimb })
            .ToListAsync(cancellationToken);
        foreach (var u in users)
        {
            var key = AvatarKeys.Key(u.CurrentKigurumi, u.CurrentHead, u.CurrentBody, u.CurrentFace, u.ColorBody, u.ColorFace, u.ColorLimb,
                animations.GetValueOrDefault(u.Baid));
            if (keys.GetValueOrDefault(u.Baid) != key) Enqueue(u.Baid);
        }
    }

    private async Task<IPage> PageAsync()
    {
        if (page is { IsClosed: false } && browser is { IsClosed: false }) return page;
        await CloseBrowserAsync();
        browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            ExecutablePath = browserPath,
            Headless = true,
            // WebGL without a GPU falls back to SwiftShader; the server's own certificate may be self-signed.
            Args = ["--enable-unsafe-swiftshader", "--ignore-certificate-errors", "--no-first-run", "--mute-audio", "--disable-extensions"],
        });
        page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 512, Height = 512 });
        // Before the embed runs: its sign-in token (it reads authToken like the WebUI) and a record of its messages.
        await page.EvaluateExpressionOnNewDocumentAsync($$"""
            localStorage.setItem("authToken", {{JsonSerializer.Serialize(JsonSerializer.Serialize(token))}});
            window.__don = null;
            addEventListener("message", e => {
              if (e.data?.type === "don-status") window.__don = e.data;
              if (e.data?.type === "don-snapshot" && e.data.dataUrl) window.__snap = e.data.dataUrl;
            });
            """);
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
