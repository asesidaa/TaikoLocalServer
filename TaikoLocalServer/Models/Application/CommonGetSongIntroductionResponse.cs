using SharedProject.Models;

namespace TaikoLocalServer.Models.Application;

public class CommonGetSongIntroductionResponse
{
    public uint Result { get; set; }

    public List<SongIntroductionData> ArySongIntroductionDatas { get; set; } = [];
}