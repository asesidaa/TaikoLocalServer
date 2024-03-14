using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class MyDonEntryMappers
{
    public static partial MydonEntryResponse MapTo3906(CommonMyDonEntryResponse response);
    
    public static partial Models.v3209.MydonEntryResponse MapTo3209(CommonMyDonEntryResponse response);
}