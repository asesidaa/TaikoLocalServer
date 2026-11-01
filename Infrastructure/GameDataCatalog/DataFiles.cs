namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

/// <summary>
/// The server-owned JSON datatables in <c>wwwroot/data</c>. Fixed names (they used to be configurable in
/// DataSettings.json, which nobody needed), so the game data editors always know where they are.
/// </summary>
public static class DataFiles
{
    public const string Dan = "dan_data.json";
    public const string Gaiden = "gaiden_data.json";
    public const string EventFolder = "event_folder_data.json";
    public const string Telop = "telop_data.json";
    public const string Intro = "intro_data.json";
    public const string Movie = "movie_data.json";
    public const string ShopFolder = "shop_folder_data.json";
    public const string Token = "token_data.json";
    public const string LockedSongs = "locked_songs_data.json";
    public const string SpecialSongs = "special_songs_data.json";
    public const string QrCode = "qrcode_data.json";
    public const string LockedCostume = "locked_costume_data.json";
    public const string LockedTitle = "locked_title_data.json";
}
