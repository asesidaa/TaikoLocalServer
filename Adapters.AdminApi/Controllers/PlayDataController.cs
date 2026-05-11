using Riok.Mapperly.Abstractions;
using Swan.Mapping;
using Throw;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);

        var songBestDbData = await context.SongBestDataNijiiro.Where(d => d.Baid == baid).ToListAsync();
        var songBestRecords = songBestDbData.Select(d => d.CopyPropertiesToNew<SongBestData>()).ToList();
        var aiSectionBest = await context.AiScoreDataNijiiro
            .Where(d => d.Baid == baid)
            .Include(d => d.AiSectionScoreData)
            .ToListAsync();
        var songPlayData = await context.SongPlayDataNijiiro.Where(d => d.Baid == baid).ToListAsync();

        foreach (var bestData in songBestRecords)
        {
            var songPlayDatums = songPlayData
                .Where(d => d.Difficulty == bestData.Difficulty && d.SongId == bestData.SongId)
                .ToArray();
            songPlayDatums.Throw($"Play log for song id {bestData.SongId} is null! Something is wrong with db!").IfEmpty();

            bestData.LastPlayTime = songPlayDatums.MaxBy(d => d.PlayTime)!.PlayTime;

            var bestLog = songPlayDatums.MaxBy(d => d.Score);
            bestLog.CopyOnlyPropertiesTo(bestData,
                nameof(SongPlayDatumNijiiro.PlayTime),
                nameof(SongPlayDatumNijiiro.GoodCount),
                nameof(SongPlayDatumNijiiro.OkCount),
                nameof(SongPlayDatumNijiiro.MissCount),
                nameof(SongPlayDatumNijiiro.HitCount),
                nameof(SongPlayDatumNijiiro.DrumrollCount),
                nameof(SongPlayDatumNijiiro.ComboCount));

            if (bestLog is not null)
            {
                bestData.PlaySetting = PlaySettingConverter.ShortToPlaySetting((short)bestLog.OptionSetting);
            }

            var aiSection = aiSectionBest.FirstOrDefault(d =>
                d.Difficulty == bestData.Difficulty && d.SongId == bestData.SongId);
            if (aiSection is null)
            {
                continue;
            }

            bestData.AiSectionBestData = aiSection.AiSectionScoreData
                .Select(d => d.CopyPropertiesToNew<AiSectionBestData>())
                .ToList();
        }

        foreach (var songBestData in songBestRecords)
        {
            var songPlayLogs = songPlayData
                .Where(d => d.SongId == songBestData.SongId && d.Difficulty == songBestData.Difficulty)
                .ToList();
            songBestData.PlayCount = songPlayLogs.Count;
            songBestData.ClearCount = songPlayLogs.Count(d => d.Crown >= CrownType.Clear);
            songBestData.FullComboCount = songPlayLogs.Count(d => d.Crown >= CrownType.Gold);
            songBestData.PerfectCount = songPlayLogs.Count(d => d.Crown >= CrownType.Dondaful);
        }

        var favoriteSet = saveData.FavoriteSongsArray.ToHashSet();
        foreach (var songBestRecord in songBestRecords.Where(r => favoriteSet.Contains(r.SongId)))
        {
            songBestRecord.IsFavorite = true;
        }

        foreach (var songBestRecord in songBestRecords)
        {
            songBestRecord.RecentPlayData = songPlayData
                .Where(d => d.SongId == songBestRecord.SongId && d.Difficulty == songBestRecord.Difficulty)
                .Select(SongBestResponseMapper.MapToDto)
                .ToList();
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
    public static partial SongPlayDatumDto MapToDto(SongPlayDatumNijiiro entity);
}
