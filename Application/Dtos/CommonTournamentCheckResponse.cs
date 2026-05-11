namespace TaikoLocalServer.Application.Dtos;

public class CommonTournamentCheckResponse
{
    public uint Result { get; set; } = 1;
    public uint RareRate { get; set; }
    public uint SongHashVer { get; set; }
    public List<GachainfoData> AryGachaSongData { get; set; } = [];
    public List<GachainfoData> AryGachaToneData { get; set; } = [];
    public List<GachainfoData> AryGachaCostume1Data { get; set; } = [];
    public List<GachainfoData> AryGachaCostume2Data { get; set; } = [];
    public List<GachainfoData> AryGachaCostume3Data { get; set; } = [];
    public List<GachainfoData> AryGachaCostume4Data { get; set; } = [];
    public List<GachainfoData> AryGachaCostume5Data { get; set; } = [];
    public List<GachainfoData> AryGachaTitleData { get; set; } = [];

    public class GachainfoData
    {
        public byte[] NormalGachaFlg { get; set; } = [];
        public byte[] RareGachaFlg { get; set; } = [];
    }
}
