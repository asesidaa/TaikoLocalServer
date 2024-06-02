namespace TaikoLocalServer.Common;

public static class Constants
{
	public const string DateTimeFormat = "yyyyMMddHHmmss";

	public const int MusicIdMax = 1600;

	public const int MusicIdMaxExpanded = 9000;

	public const string DefaultDbName = "taiko.db3";

	public const string MusicInfoBaseName = "musicinfo";
	public const string WordlistBaseName = "wordlist";
	public const string MusicOrderBaseName = "music_order";
	public const string DonCosRewardBaseName = "don_cos_reward";
	public const string ShougouBaseName = "shougou";
	public const string NeiroBaseName = "neiro";

	public const uint DanVerupMasterType = 101;
	public const uint GaidenVerupMasterType = 102;
	public const uint FolderVerupMasterType = 103;
	public const uint IntroVerupMasterType = 105;

	public const uint FunctionIdDaniFolderAvailable = 1;
	public const uint FunctionIdDaniAvailable = 2;
	public const uint FunctionIdAiBattleAvailable = 3;
}