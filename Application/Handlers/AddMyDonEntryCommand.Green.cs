namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial ValueTask<CommonMyDonEntryResponse> HandleGreen(AddMyDonEntryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green AddMyDonEntry stub for access code {AccessCode}, returning success", request.AccessCode);

        return ValueTask.FromResult(new CommonMyDonEntryResponse
        {
            Result = 1,
            MydonName = request.Name,
            MydonNameLanguage = request.Language,
            ComSvrResult = 1,
            AccessCode = request.AccessCode
        });
    }
}
