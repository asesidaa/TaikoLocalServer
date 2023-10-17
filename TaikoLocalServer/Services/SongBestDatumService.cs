using GameDatabase.Context;
using GameDatabase.Entities;
using SharedProject.Models;
using Swan.Mapping;
using Throw;

namespace TaikoLocalServer.Services;

public class SongBestDatumService : ISongBestDatumService
{
    private readonly TaikoDbContext context;

    public SongBestDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<List<SongBestDatum>> GetAllSongBestData(ulong baid)
    {
        return await context.SongBestData.Where(datum => datum.Baid == baid).ToListAsync();
    }

    public async Task<SongBestDatum?> GetSongBestData(ulong baid, uint songId, Difficulty difficulty)
    {
        return await context.SongBestData.Where(datum => datum.Baid == baid &&
                                                        datum.SongId == songId &&
                                                        datum.Difficulty == difficulty)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateSongBestData(SongBestDatum datum)
    {
        var existing = await context.SongBestData.FindAsync(datum.Baid, datum.SongId, datum.Difficulty);
        existing.ThrowIfNull("Cannot update a non-existing best data!");

        context.SongBestData.Update(datum);
        await context.SaveChangesAsync();
    }

    public async Task InsertSongBestData(SongBestDatum datum)
    {
        var existing = await context.SongBestData.FindAsync(datum.Baid, datum.SongId, datum.Difficulty);
        if (existing is not null)
        {
            throw new ArgumentException("Best data already exists!", nameof(datum));
        }
        context.SongBestData.Add(datum);
        await context.SaveChangesAsync();
    }

    public async Task<List<SongBestData>> GetAllSongBestAsModel(ulong baid)
    {
        var songbestDbData = await context.SongBestData.Where(datum => datum.Baid == baid)
            .ToListAsync();

        var result = songbestDbData.Select(datum => datum.CopyPropertiesToNew<SongBestData>()).ToList();

        var aiSectionBest = await context.AiScoreData.Where(datum => datum.Baid == baid)
            .Include(datum => datum.AiSectionScoreData)
            .ToListAsync();

        var playLogs = await context.SongPlayData.Where(datum => datum.Baid == baid).ToListAsync();
        foreach (var bestData in result)
        {
            var songPlayDatums = playLogs.Where(datum => datum.Difficulty == bestData.Difficulty &&
                                                         datum.SongId == bestData.SongId).ToArray();
            songPlayDatums.Throw($"Play log for song id {bestData.SongId} is null! " +
                                 "Something is wrong with db!")
                .IfEmpty();
            var lastPlayLog = songPlayDatums
                .MaxBy(datum => datum.PlayTime);
            bestData.LastPlayTime = lastPlayLog!.PlayTime;
            
            var bestLog = songPlayDatums
                .MaxBy(datum => datum.Score);
            bestLog.CopyOnlyPropertiesTo(bestData,
                nameof(SongPlayDatum.GoodCount),
                nameof(SongPlayDatum.OkCount),
                nameof(SongPlayDatum.MissCount),
                nameof(SongPlayDatum.HitCount),
                nameof(SongPlayDatum.DrumrollCount),
                nameof(SongPlayDatum.ComboCount)
            );

            var aiSection = aiSectionBest.FirstOrDefault(datum => datum.Difficulty == bestData.Difficulty &&
                                                         datum.SongId == bestData.SongId);
            if (aiSection is null)
            {
                continue;
            }

            bestData.AiSectionBestData = aiSection.AiSectionScoreData
                .Select(datum => datum.CopyPropertiesToNew<AiSectionBestData>()).ToList();
        }

        return result;
    }
}