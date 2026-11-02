namespace TaikoLocalServer.Application.Dtos;

public class CommonItemShopInfoResponse
{
    public uint Result { get; set; } = 1;
    public uint VerupNo { get; set; }
    public uint SeasonId { get; set; }
    public string Telop { get; set; } = string.Empty;
    public string StartDatetime { get; set; } = string.Empty;
    public string EndDatetime { get; set; } = string.Empty;
    public uint AfterstartDays { get; set; }
    public uint BeforecloseDays { get; set; }
    public List<ItemShopData> AryItemshopData { get; set; } = [];

    public class ItemShopData
    {
        public uint ItemNo { get; set; }
        public uint ItemType { get; set; }
        public uint ItemId { get; set; }
        public uint ItemPrice { get; set; }
    }
}
