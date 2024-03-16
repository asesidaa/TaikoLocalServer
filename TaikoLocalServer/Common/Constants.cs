namespace TaikoLocalServer.Common;

public static class Constants
{
	public const string DATE_TIME_FORMAT = "yyyyMMddHHmmss";

	public const int MUSIC_ID_MAX = 1600;

	public const int MUSIC_ID_MAX_EXPANDED = 9000;

	public const string DEFAULT_DB_NAME = "taiko.db3";

	public const string MUSIC_INFO_BASE_NAME = "musicinfo";
	public const string WORDLIST_BASE_NAME = "wordlist";
	public const string MUSIC_ORDER_BASE_NAME = "music_order";
	public const string DON_COS_REWARD_BASE_NAME = "don_cos_reward";
	public const string SHOUGOU_BASE_NAME = "shougou";
	public const string NEIRO_BASE_NAME = "neiro";

	public const uint DAN_VERUP_MASTER_TYPE = 101;
	public const uint GAIDEN_VERUP_MASTER_TYPE = 102;
	public const uint FOLDER_VERUP_MASTER_TYPE = 103;
	public const uint INTRO_VERUP_MASTER_TYPE = 105;

	public const uint FUNCTION_ID_DANI_FOLDER_AVAILABLE = 1;
	public const uint FUNCTION_ID_DANI_AVAILABLE = 2;
	public const uint FUNCTION_ID_AI_BATTLE_AVAILABLE = 3;
}