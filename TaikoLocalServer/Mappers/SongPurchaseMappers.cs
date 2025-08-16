using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class SongPurchaseMappers
{
    public static partial SongPurchaseResponse MapToWW08(CommonSongPurchaseResponse response);
    
    public static partial Models.CN00.SongPurchaseResponse MapToCN00(CommonSongPurchaseResponse response);
    
    public static partial PurchaseSongCommand MapToCommand(SongPurchaseRequest request);

    public static partial PurchaseSongCommandCN MapToCommand(Models.CN00.SongPurchaseRequest request);
}