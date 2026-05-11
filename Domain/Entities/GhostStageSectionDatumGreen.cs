namespace TaikoLocalServer.Domain.Entities;

public class GhostStageSectionDatumGreen
{
    public long PlayId { get; set; }
    public uint SectionNo { get; set; }
    public bool IsWin { get; set; }
    public uint GoodCount { get; set; }
    public uint OkCount { get; set; }
    public uint NgCount { get; set; }
    public uint PoundCount { get; set; }
    public SongPlayDatumGreen Parent { get; set; } = null!;
}
