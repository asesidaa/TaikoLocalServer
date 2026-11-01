using Riok.Mapperly.Abstractions;
using Swan.Mapping;
using Throw;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class PlayDataController(ITaikoDbContext context) : BaseAdminController<PlayDataController>
{
    [HttpGet("{baid}")]
    public async Task<ActionResult<SongBestResponse>> GetSongBestRecords(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var songBestDbData = await context.SongBestData.Where(d => d.Baid == baid).ToListAsync();
        var songBestRecords = songBestDbData.Select(d => d.CopyPropertiesToNew<SongBestData>()).ToList();
        var aiSectionBest = await context.AiScoreData
            .Where(d => d.Baid == baid)
            .Include(d => d.AiSectionScoreData)
            .ToListAsync();
        // Each chart's plays, grouped once: looking them up per best scanned every play per best (5,000 × 100,000).
        var playsByChart = (await context.SongPlayData.Where(d => d.Baid == baid).ToListAsync())
            .ToLookup(d => (d.SongId, d.Difficulty));
        var aiByChart = aiSectionBest.ToDictionary(d => (d.SongId, d.Difficulty));

        foreach (var bestData in songBestRecords)
        {
            var songPlayDatums = playsByChart[(bestData.SongId, bestData.Difficulty)].ToArray();
            songPlayDatums.Throw($"Play log for song id {bestData.SongId} is null! Something is wrong with db!").IfEmpty();

            bestData.LastPlayTime = songPlayDatums.MaxBy(d => d.PlayTime)!.PlayTime;

            var bestLog = songPlayDatums.MaxBy(d => d.Score);
            bestLog.CopyOnlyPropertiesTo(bestData,
                nameof(SongPlayDatum.PlayTime),
                nameof(SongPlayDatum.GoodCount),
                nameof(SongPlayDatum.OkCount),
                nameof(SongPlayDatum.MissCount),
                nameof(SongPlayDatum.HitCount),
                nameof(SongPlayDatum.DrumrollCount),
                nameof(SongPlayDatum.ComboCount));

            if (bestLog is not null)
            {
                bestData.PlaySetting = PlaySettingConverter.ShortToPlaySetting((short)bestLog.OptionSetting);
            }

            bestData.PlayCount = songPlayDatums.Length;
            bestData.ClearCount = songPlayDatums.Count(d => d.Crown >= CrownType.Clear);
            bestData.FullComboCount = songPlayDatums.Count(d => d.Crown >= CrownType.Gold);
            bestData.PerfectCount = songPlayDatums.Count(d => d.Crown >= CrownType.Dondaful);
            bestData.RecentPlayData = songPlayDatums.Select(SongBestResponseMapper.MapToDto).ToList();

            if (aiByChart.TryGetValue((bestData.SongId, bestData.Difficulty), out var aiSection))
            {
                bestData.AiSectionBestData = aiSection.AiSectionScoreData
                    .Select(d => d.CopyPropertiesToNew<AiSectionBestData>())
                    .ToList();
            }
        }

        var favoriteSet = user.FavoriteSongsArray.ToHashSet();
        foreach (var songBestRecord in songBestRecords.Where(r => favoriteSet.Contains(r.SongId)))
        {
            songBestRecord.IsFavorite = true;
        }

        return Ok(new SongBestResponse
        {
            SongBestData = songBestRecords
        });
    }
}

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public partial class SongBestResponseMapper
{
    public static partial SongPlayDatumDto MapToDto(SongPlayDatum entity);
}
