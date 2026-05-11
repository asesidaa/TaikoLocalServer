namespace TaikoLocalServer.Application.Dtos;

public sealed class CommonTaikojukuResponse
{
    public uint Result { get; set; } = 1;

    public List<Pack> Packs { get; set; } = [];

    public sealed class Pack
    {
        public uint GetDan { get; set; }

        public uint VerupNo { get; set; }

        public List<Song> Songs { get; set; } = [];
    }

    public sealed class Song
    {
        public uint SongNo { get; set; }

        public uint Level { get; set; }
    }
}
