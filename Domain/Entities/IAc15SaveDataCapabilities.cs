namespace TaikoLocalServer.Domain.Entities;

public interface IAc15MedalSaveData
{
    uint TotalGetDonmedal { get; set; }

    uint TotalUseDonmedal { get; set; }

    uint TotalGetKatsumedal { get; set; }

    uint TotalUseKatsumedal { get; set; }
}

public interface IAc15TutorialSaveData
{
    uint ItemshopTutorialFlg { get; set; }

    bool IsDevil { get; set; }

    bool IsExplain { get; set; }

    uint WaiwaiTutorialFlg { get; set; }

    uint DifficultyPlayedCourse { get; set; }

    uint DifficultyPlayedStar { get; set; }
}

public interface IAc15PlayProfileSaveData
{
    DateTime LastPlayDatetime { get; set; }

    uint PrevAreaCode { get; set; }
}

public interface IAc15CustomizationSaveData
{
    bool IsAutoCostumeOn { get; set; }

    uint Costume1 { get; set; }

    uint Costume2 { get; set; }

    uint Costume3 { get; set; }

    uint Costume4 { get; set; }

    uint Costume5 { get; set; }

    byte[] CostumeFlg1 { get; set; }

    byte[] CostumeFlg2 { get; set; }

    byte[] CostumeFlg3 { get; set; }

    byte[] CostumeFlg4 { get; set; }

    byte[] CostumeFlg5 { get; set; }

    byte[] ToneFlg { get; set; }

    byte[] TitleFlg { get; set; }
}

public interface IAc15SongUnlockSaveData
{
    byte[] ReleaseSongFlg { get; set; }
}
