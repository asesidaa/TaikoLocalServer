using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class FolderDataMappers
{
    public static partial GetfolderResponse MapTo3906(CommonGetFolderResponse response);
    
    public static partial Models.v3209.GetfolderResponse MapTo3209(CommonGetFolderResponse response);
}