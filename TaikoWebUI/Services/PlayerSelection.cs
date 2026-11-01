using System.Text.RegularExpressions;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Routing;

namespace TaikoWebUI.Services;

/// <summary>
/// Local mode: whose data the player pages show (there's no signed-in user). Remembered on this device.
/// Opening a /Users/{baid}/… page makes that player current, so the switcher always names the data on screen.
/// </summary>
public sealed partial class PlayerSelection(HttpClient client, ILocalStorageService storage, NavigationManager nav) : IDisposable
{
    private const string Key = "ui.player";

    public uint Baid { get; private set; }
    public IReadOnlyList<User> Users { get; private set; } = [];
    public User? Current => Users.FirstOrDefault(u => u.Baid == Baid);
    public event Action? Changed;

    private bool loaded;

    public async Task LoadAsync()
    {
        if (loaded) return;
        loaded = true;
        Users = await GetAllUsersAsync(client);
        var stored = await storage.GetItemAsync<uint?>(Key);
        Baid = RouteBaid() ?? (Users.Any(u => u.Baid == stored) ? stored!.Value : Users.FirstOrDefault()?.Baid ?? 0);
        nav.LocationChanged += OnLocationChanged;
        Changed?.Invoke();
    }

    /// <summary>Switch player; on a player page, open the same page for them.</summary>
    public async Task SelectAsync(uint baid)
    {
        await SetAsync(baid);
        var path = nav.ToBaseRelativePath(nav.Uri);
        var m = UserRoute().Match(path);
        if (m.Success) nav.NavigateTo($"/Users/{baid}{path[m.Length..]}");
    }

    private async Task SetAsync(uint baid)
    {
        if (baid == Baid) return;
        Baid = baid;
        await storage.SetItemAsync(Key, baid);
        Changed?.Invoke();
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (RouteBaid() is { } baid) await SetAsync(baid);
    }

    private uint? RouteBaid()
    {
        var m = UserRoute().Match(nav.ToBaseRelativePath(nav.Uri));
        return m.Success && uint.TryParse(m.Groups[1].Value, out var b) ? b : null;
    }

    [GeneratedRegex(@"^Users/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex UserRoute();

    /// <summary>Every user, 200 a page.</summary>
    public static async Task<List<User>> GetAllUsersAsync(HttpClient client)
    {
        var all = new List<User>();
        for (var page = 1; ; page++)
        {
            var res = await client.GetFromJsonAsync<UsersResponse>($"api/Users?page={page}&limit=200");
            all.AddRange(res?.Users ?? []);
            if (res is null || page >= res.TotalPages) return all;
        }
    }

    public void Dispose() => nav.LocationChanged -= OnLocationChanged;
}
