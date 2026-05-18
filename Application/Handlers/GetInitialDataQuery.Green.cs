namespace TaikoLocalServer.Application.Handlers;

public partial class GetInitialDataQueryHandler
{
    private partial ValueTask<CommonInitialDataCheckResponse> HandleGreen(GetInitialDataQuery request, CancellationToken cancellationToken)
    {
        var green = gameDataService.Green();
        var allSongs = green.MusicInfoFileOrder.Select(song => song.SongNo);

        return ValueTask.FromResult(new CommonInitialDataCheckResponse
        {
            Result = 1,
            DefaultSongFlg = GreenProtocolBytes.CreateFixedBitset(allSongs, GreenProtocolBytes.SongFlagBytes),
            AchievementSongBit = new byte[GreenProtocolBytes.SongFlagBytes],
            UraReleaseBit = new byte[GreenProtocolBytes.SongFlagBytes],
            SongHashVer = green.SongHashVersion,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = green.ItemShop.Count > 0,
            IsGhostbattleplay = true,
            AryGreenTelopDatas = green.Telops.Values
                .OrderBy(entry => entry.TelopId)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.TelopId,
                    VerupNo = entry.VerupNo
                })
                .ToList(),
            AryGreenTaikojukuDatas = green.TaikojukuFileOrder
                .Where(entry => entry.ChallengeLevel is >= 1 and <= 25)
                .Select(entry => new CommonInitialDataCheckResponse.InformationData
                {
                    InfoId = entry.ChallengeLevel,
                    VerupNo = entry.VerupNo + 1
                })
                .ToList(),
            ServerCurrentDatetime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        });
    }
}
