using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface ISongPlayDatumService
{
	public Task<List<SongPlayDatum>> GetSongPlayDatumByBaid(ulong baid);

	public Task AddSongPlayDatum(SongPlayDatum datum);
}