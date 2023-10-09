namespace TaikoLocalServer.Common;

public static class Constants
{
	public const string DATE_TIME_FORMAT = "yyyyMMddHHmmss";

	public const int MUSIC_ID_MAX = 1599;

	public const int MUSIC_ID_MAX_EXPANDED = 9000;

	public const string DEFAULT_DB_NAME = "taiko.db3";
	
	public const string MUSIC_INFO_BASE_NAME = "musicinfo";
	public const string MUSIC_ATTRIBUTE_BASE_NAME = "music_attribute";
	public const string MUSIC_ATTRIBUTE_COMPRESSED_FILE_NAME = "music_attribute.bin";
	
	
	public const string WORDLIST_BASE_NAME = "wordlist";
	public const string MUSIC_ORDER_BASE_NAME = "music_order";

	public const string DAN_DATA_FILE_NAME = "dan_data.json";
	public const string INTRO_DATA_FILE_NAME = "intro_data.json";
	public const string EVENT_FOLDER_DATA_FILE_NAME = "event_folder_data.json";

	public const int MIN_DAN_ID = 1;
	public const int MAX_DAN_ID = 19;

	public const int TONE_UID_MAX = 19;
	
	public const int TITLE_UID_MAX = 1599;

	private const int COSTUME_FLAG_1_ARRAY_SIZE = 156; // Kigurumi
	private const int COSTUME_FLAG_2_ARRAY_SIZE = 140; // Head
	private const int COSTUME_FLAG_3_ARRAY_SIZE = 156; // Body
	private const int COSTUME_FLAG_4_ARRAY_SIZE = 58;  // Face
	private const int COSTUME_FLAG_5_ARRAY_SIZE = 142; // Puchi
	public static readonly int[] CostumeFlagArraySizes =
	{
		COSTUME_FLAG_1_ARRAY_SIZE,
		COSTUME_FLAG_2_ARRAY_SIZE,
		COSTUME_FLAG_3_ARRAY_SIZE,
		COSTUME_FLAG_4_ARRAY_SIZE,
		COSTUME_FLAG_5_ARRAY_SIZE
	};
}