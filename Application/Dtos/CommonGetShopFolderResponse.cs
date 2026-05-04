using SharedProject.Models;

namespace TaikoLocalServer.Application.Dtos;

public class CommonGetShopFolderResponse
{
    public uint                 Result             { get; set; }
    public uint                 VerupNo            { get; set; }
    public uint                 TokenId            { get; set; }
    public List<ShopFolderData> AryShopFolderDatas { get; set; } = [];
}