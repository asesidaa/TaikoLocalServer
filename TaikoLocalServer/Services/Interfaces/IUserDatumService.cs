using GameDatabase.Entities;

namespace TaikoLocalServer.Services.Interfaces;

public interface IUserDatumService
{
    public Task<UserDatum?> GetFirstUserDatumOrNull(uint baid);

    public Task UpdateUserDatum(UserDatum userDatum);

    public Task<bool> DeleteUser(uint baid);

    public Task<List<uint>> GetFavoriteSongIds(uint baid);

    public Task UpdateFavoriteSong(uint baid, uint songId, bool isFavorite);
}