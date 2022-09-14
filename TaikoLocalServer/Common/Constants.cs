namespace TaikoLocalServer.Common;

public static class Constants
{
    public const string DATE_TIME_FORMAT = "yyyyMMddHHmmss";

    public const int MUSIC_ID_MAX = 1600;

    public const int MUSIC_FLAG_ARRAY_SIZE = MUSIC_ID_MAX / 8 + 1;

    public const int CROWN_FLAG_ARRAY_SIZE = MUSIC_ID_MAX + 1;
    
    public const int DONDAFUL_CROWN_FLAG_ARRAY_SIZE = MUSIC_ID_MAX + 1;

    public const int KIWAMI_SCORE_RANK_ARRAY_SIZE = MUSIC_ID_MAX + 1;

    public const int IKI_CORE_RANK_ARRAY_SIZE = MUSIC_ID_MAX + 1;
    
    public const int MIYABI_CORE_RANK_ARRAY_SIZE = MUSIC_ID_MAX + 1;

    public const string DEFAULT_DB_NAME = "taiko.db3";

    public const string MUSIC_ATTRIBUTE_FILE_NAME = "music_attribute.json";

    public const string DAN_DATA_FILE_NAME = "dan_data.json";

    public const int MIN_DAN_ID = 1;
    public const int MAX_DAN_ID = 19;
    public const int GOT_DAN_BITS = MAX_DAN_ID * 4;

    public const int TONE_UID_MAX = 19;
    public const int TITLE_UID_MAX = 814;
    public const int COSTUME_FLAG_1_ARRAY_SIZE = 154;
    public const int COSTUME_FLAG_2_ARRAY_SIZE = 140;
    public const int COSTUME_FLAG_3_ARRAY_SIZE = 156;
    public const int COSTUME_FLAG_4_ARRAY_SIZE = 58;
    public const int COSTUME_FLAG_5_ARRAY_SIZE = 129;
    
    public static readonly int[] CostumeFlagArraySizes = {154, 140, 156, 58, 129};
}