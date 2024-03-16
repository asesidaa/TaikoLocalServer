using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class ShopFolderDataMappers
{
    public static partial GetShopFolderResponse MapTo3906(CommonGetShopFolderResponse response);
    
    public static partial Models.v3209.GetShopFolderResponse MapTo3209(CommonGetShopFolderResponse response);
}