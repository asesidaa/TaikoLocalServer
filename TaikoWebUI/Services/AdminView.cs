using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Routing;

namespace TaikoWebUI.Services;

/// <summary>
/// Server mode: an admin on another player's pages (/Users/{id}/…, opened from User Management) sees them as that
/// player does. The sidebar's player links follow that player and the layout shows the admin-view banner; leaving
/// for a page that isn't theirs (Exit, the admin pages) ends it. It comes from the address, so a reload keeps it.
/// </summary>
public sealed partial class AdminView : IDisposable
{
    private readonly AuthService auth;
    private readonly NavigationManager nav;
    private readonly HttpClient client;

    public AdminView(AuthService auth, NavigationManager nav, HttpClient client)
    {
        this.auth = auth;
        this.nav = nav;
        this.client = client;
        nav.LocationChanged += OnChanged;
        auth.LoginStatusChanged += OnChanged;
        Update();
    }

    /// <summary>The player being viewed; 0 when it's the admin's own pages (or no admin view).</summary>
    public uint Baid { get; private set; }
    public string Name { get; private set; } = "";
    public event Action? Changed;

    private void OnChanged(object? sender, EventArgs e) => Update();

    private async void Update()
    {
        var m = UserRoute().Match(nav.ToBaseRelativePath(nav.Uri));
        var baid = auth.LoginRequired && auth.IsAdmin && m.Success && uint.TryParse(m.Groups[1].Value, out var b) && b != auth.GetLoggedInBaid() ? b : 0;
        if (baid == Baid) return;
        Baid = baid;
        Name = "";
        Changed?.Invoke();
        if (baid == 0) return;
        try
        {
            var name = (await client.GetFromJsonAsync<User>($"api/Users/{baid}"))?.UserSetting.MyDonName;
            if (Baid != baid) return;
            Name = string.IsNullOrEmpty(name) ? $"#{baid}" : name;
            Changed?.Invoke();
        }
        catch (HttpRequestException) { /* the banner names them by ID */ }
    }

    [GeneratedRegex(@"^Users/(\d+)(?:/|$)", RegexOptions.IgnoreCase)]
    private static partial Regex UserRoute();

    public void Dispose()
    {
        nav.LocationChanged -= OnChanged;
        auth.LoginStatusChanged -= OnChanged;
    }
}
