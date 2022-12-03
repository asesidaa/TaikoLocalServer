using GameDatabase.Context;
using GameDatabase.Entities;
using Throw;

namespace TaikoLocalServer.Services;

public class AiDatumService : IAiDatumService
{
    private readonly TaikoDbContext context;

    public AiDatumService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<List<AiScoreDatum>> GetAllAiScoreById(uint baid)
    {
        return await context.AiScoreData.Where(datum => datum.Baid == baid)
            .Include(datum => datum.AiSectionScoreData)
            .ToListAsync();
    }

    public async Task<AiScoreDatum?> GetSongAiScore(uint baid, uint songId, Difficulty difficulty)
    {
        return await context.AiScoreData.Where(datum => datum.Baid == baid &&
                                                        datum.SongId == songId &&
                                                        datum.Difficulty == difficulty)
            .Include(datum => datum.AiSectionScoreData)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateSongAiScore(AiScoreDatum datum)
    {
        var existing = await context.AiScoreData.FindAsync(datum.Baid, datum.SongId, datum.Difficulty);
        existing.ThrowIfNull("Cannot update a non-existing ai score!");

        context.AiScoreData.Update(datum);
        await context.SaveChangesAsync();
    }

    public async Task InsertSongAiScore(AiScoreDatum datum)
    {
        var existing = await context.AiScoreData.FindAsync(datum.Baid, datum.SongId, datum.Difficulty);
        if (existing is not null) throw new ArgumentException("Ai score already exists!", nameof(datum));
        context.AiScoreData.Add(datum);
        await context.SaveChangesAsync();
    }
}