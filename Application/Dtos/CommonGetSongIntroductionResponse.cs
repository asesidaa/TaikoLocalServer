
namespace TaikoLocalServer.Application.Dtos;

public class CommonGetSongIntroductionResponse
{
    public uint Result { get; set; }

    public List<SongIntroductionData> ArySongIntroductionDatas { get; set; } = [];
}