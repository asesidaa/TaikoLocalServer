using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class SongPurchaseMappers
{
    public static partial SongPurchaseResponse MapToWW08(CommonSongPurchaseResponse response);

    public static partial PurchaseSongCommand MapToCommand(SongPurchaseRequest request);
}
