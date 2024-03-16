using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Handlers;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class SongPurchaseMappers
{
    public static partial SongPurchaseResponse MapTo3906(CommonSongPurchaseResponse response);
    
    public static partial Models.v3209.SongPurchaseResponse MapTo3209(CommonSongPurchaseResponse response);

    public static partial PurchaseSongCommand MapToCommand(SongPurchaseRequest request);

    public static partial PurchaseSongCommand MapToCommand(Models.v3209.SongPurchaseRequest request);
}