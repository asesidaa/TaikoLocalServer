namespace TaikoLocalServer.Settings;

public class DataSettings
{
	public string DanDataFileName { get; set; } = "dan_data.json";
	
	public string GaidenDataFileName { get; set; } = "gaiden_data.json";

	public string EventFolderDataFileName { get; set; } = "event_folder_data.json";

	public string IntroDataFileName { get; set; } = "intro_data.json";

	public string MovieDataFileName { get; set; } = "movie_data.json";

	public string ShopFolderDataFileName { get; set; } = "shop_folder_data.json";

	public string TokenDataFileName { get; set; } = "token_data.json";

	public string LockedSongsDataFileName { get; set; } = "locked_songs_data.json";
}