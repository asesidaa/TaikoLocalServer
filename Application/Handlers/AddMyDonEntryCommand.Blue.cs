using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial async ValueTask<CommonMyDonEntryResponse> HandleBlue(
        AddMyDonEntryCommand request,
        CancellationToken cancellationToken)
    {
        return await Ac15MyDonEntryService.HandleAsync(
            context,
            request.AccessCode,
            request.Name,
            request.Language,
            context.UserSaveDataBlue,
            UserSaveDataBlueExtensions.CreateDefaultBlueSaveData,
            logger,
            nameof(GameEra.Blue),
            cancellationToken);
    }
}
