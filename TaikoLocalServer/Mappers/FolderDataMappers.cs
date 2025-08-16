using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    public static partial GetfolderResponse MapToWW08(CommonGetFolderResponse response);
    
    public static partial Models.CN00.GetfolderResponse MapToCN00(CommonGetFolderResponse response);
}