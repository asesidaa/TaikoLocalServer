using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface IUserDatumService
{
    public Task<List<UserDatum>> GetAllUserDatum();

    public Task<Dictionary<uint, User>> GetAllUserDict();

    public Task<UserDatum?> GetFirstUserDatumOrNull(uint baid);

    public Task UpdateUserDatum(UserDatum userDatum);

    public Task<bool> DeleteUser(uint baid);

    public Task<List<uint>> GetFavoriteSongIds(uint baid);

    public Task UpdateFavoriteSong(uint baid, uint songId, bool isFavorite);
}