using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class ShopFolderDataMappers
{
    public static partial GetShopFolderResponse MapToWW08(CommonGetShopFolderResponse response);
    
    public static partial Models.CN00.GetShopFolderResponse MapToCN00(CommonGetShopFolderResponse response);
}