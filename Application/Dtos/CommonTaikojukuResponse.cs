namespace TaikoLocalServer.Application.Dtos;

public class CommonTaikojukuResponse
{
    public uint Result { get; set; } = 1;
    public List<JukupackData> AryJukupackData { get; set; } = [];

    public class JukupackData
    {
        public uint GetDan { get; set; }
        public uint VerupNo { get; set; }
        public List<JukusongData> AryJukusongData { get; set; } = [];
    }

    public class JukusongData
    {
        public uint SongNo { get; set; }
        public uint Level { get; set; }
    }
}
