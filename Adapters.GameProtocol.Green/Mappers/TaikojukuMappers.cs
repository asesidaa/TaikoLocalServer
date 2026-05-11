using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class TaikojukuMappers
{
    public static TaikojukuResponse Map(CommonTaikojukuResponse common)
    {
        var response = new TaikojukuResponse { Result = common.Result };

        foreach (var pack in common.Packs)
        {
            var wirePack = new TaikojukuResponse.JukupackData
            {
                GetDan = pack.GetDan,
                VerupNo = pack.VerupNo
            };

            foreach (var song in pack.Songs)
            {
                wirePack.AryJukusongDatas.Add(new TaikojukuResponse.JukupackData.JukusongData
                {
                    SongNo = song.SongNo,
                    Level = song.Level
                });
            }

            response.AryJukupackDatas.Add(wirePack);
        }

        return response;
    }
}
