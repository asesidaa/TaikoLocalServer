namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class DanBestData
{
    public uint DanId { get; set; }
    
    public DanClearState ClearState { get; set; }

    public uint SoulGaugeTotal { get; set; }

    public uint ComboCountTotal { get; set; }

    /// <summary>Runs counted (0 for bests from before the server counted them).</summary>
    public uint PlayCount { get; set; }

    /// <summary>When the best run was played; null when unknown (older bests).</summary>
    public DateTime? BestRunAt { get; set; }

    public List<DanBestStageData> DanBestStageDataList { get; set; } = new();
}