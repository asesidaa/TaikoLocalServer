using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class ShopFolderDataMappers
{
    public static partial GetShopFolderResponse MapToWW08(CommonGetShopFolderResponse response);
}
