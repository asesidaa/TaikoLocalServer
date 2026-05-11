namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleGreen(GetInitialDataQuery request, CancellationToken cancellationToken)
    {
        var green = gameDataService.Green();
        var firstTen = green.MusicInfoFileOrder.Take(10).Select(song => song.SongNo);

        return ValueTask.FromResult(new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = GreenProtocolBytes.CreateFixedBitset(firstTen, GreenProtocolBytes.SongFlagBytes),
            AchievementSongBit = new byte[GreenProtocolBytes.SongFlagBytes],
            UraReleaseBit = new byte[GreenProtocolBytes.SongFlagBytes],
            SongHashVer = green.SongHashVersion,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = true,
            IsGhostbattleplay = true,
            AryGreenTaikojukuDatas = green.TaikojukuFileOrder.Take(3)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.UniqueId != 0 ? entry.UniqueId : entry.ChallengeLevel,
                    VerupNo = entry.VerupNo
                })
                .ToList(),
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        });
    }
}
