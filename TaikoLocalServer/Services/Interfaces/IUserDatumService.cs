using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IUserDatumService
{
    public Task<UserDatum?> GetFirstUserDatumOrNull(uint baid);

    public Task<UserDatum> GetFirstUserDatumOrDefault(uint baid);

    public Task<List<UserDatum>> GetAllUserData();

    public Task UpdateOrInsertUserDatum(UserDatum userDatum);

    public Task InsertUserDatum(UserDatum userDatum);

    public Task UpdateUserDatum(UserDatum userDatum);

    public Task<List<uint>> GetFavoriteSongIds(uint baid);

    public Task UpdateFavoriteSong(uint baid, uint songId, bool isFavorite);
}