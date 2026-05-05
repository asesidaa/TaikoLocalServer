namespace TaikoLocalServer.Contracts.AdminApi.Responses;

public class SongHistoryResponse
{
    public List<SongHistoryData> SongHistoryData { get; set; } = new();
}
