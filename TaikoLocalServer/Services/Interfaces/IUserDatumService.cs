using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IUserDatumService
{
	public Task<UserDatum?> GetFirstUserDatumOrNull(ulong baid);

	public Task<UserDatum> GetFirstUserDatumOrDefault(ulong baid);

	public Task<List<UserDatum>> GetAllUserData();

	public Task UpdateOrInsertUserDatum(UserDatum userDatum);

	public Task InsertUserDatum(UserDatum userDatum);

	public Task UpdateUserDatum(UserDatum userDatum);

	public Task<bool> DeleteUser(uint baid);

	public Task<List<uint>> GetFavoriteSongIds(ulong baid);

	public Task UpdateFavoriteSong(ulong baid, uint songId, bool isFavorite);
}