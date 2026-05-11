using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class SongPurchaseMappers
{
    public static partial SongPurchaseResponse MapToCN00(CommonSongPurchaseResponse response);

    public static PurchaseSongCommandCN MapToCommand(SongPurchaseRequest request) =>
        new((uint)request.Baid, GameEra.Nijiiro, request.SongNo, request.TokenId, request.Price);
}
