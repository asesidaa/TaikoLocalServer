using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class ShopFolderDataMappers
{
    public static partial GetShopFolderResponse MapToCN00(CommonGetShopFolderResponse response);
}
