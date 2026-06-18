using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial async ValueTask<CommonMyDonEntryResponse> HandleWhite(
        AddMyDonEntryCommand request,
        CancellationToken cancellationToken)
    {
        return await Ac15MyDonEntryService.HandleAsync(
            context,
            request.AccessCode,
            request.Name,
            request.Language,
            context.UserSaveDataWhite,
            UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData,
            logger,
            nameof(GameEra.White),
            cancellationToken);
    }
}
