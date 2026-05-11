namespace TaikoLocalServer.Application.Dtos;

public partial class CommonBaidResponse
{
    public byte[]? CostumeFlg1 { get; set; }
    public byte[]? CostumeFlg2 { get; set; }
    public byte[]? CostumeFlg3 { get; set; }
    public byte[]? CostumeFlg4 { get; set; }
    public byte[]? CostumeFlg5 { get; set; }

    public uint?   TotalGetDonmedal     { get; set; }
    public uint?   TotalUseDonmedal     { get; set; }
    public uint?   TotalGetKatsumedal   { get; set; }
    public uint?   TotalUseKatsumedal   { get; set; }
    public uint?   ItemshopTutorialFlg  { get; set; }
    public bool?   IsAutoCostumeOn      { get; set; }
    public uint?   DispDanType          { get; set; }
    public byte[]? GotDanExtraFlg       { get; set; }
    public uint?   DefaultToneSetting   { get; set; }
    public string? PersonId             { get; set; }
    public uint?   WaiwaiTutorialFlg    { get; set; }
    public List<GreenCostumeSlots> AryFavoriteCostumeData { get; set; } = [];

    public class GreenCostumeSlots
    {
        public uint Costume1 { get; set; }
        public uint Costume2 { get; set; }
        public uint Costume3 { get; set; }
        public uint Costume4 { get; set; }
        public uint Costume5 { get; set; }
    }
}
