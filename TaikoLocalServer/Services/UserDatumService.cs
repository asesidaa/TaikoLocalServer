using GameDatabase.Context;
using GameDatabase.Entities;
using Throw;

namespace TaikoLocalServer.Services;

public class UserDatumService : IUserDatumService
{
    private readonly TaikoDbContext context;

    private readonly ILogger<UserDatumService> logger;

    public UserDatumService(TaikoDbContext context, ILogger<UserDatumService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<UserDatum?> GetFirstUserDatumOrNull(uint baid)
    {
        return await context.UserData
            .Include(d => d.Tokens)
            .FirstOrDefaultAsync(d => d.Baid == baid);
    }

    public async Task UpdateUserDatum(UserDatum userDatum)
    {
        context.Update(userDatum);
        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteUser(uint baid)
    {
        var userDatum = await context.UserData.FindAsync(baid);
        if (userDatum == null) return false;
        context.UserData.Remove(userDatum);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<List<uint>> GetFavoriteSongIds(uint baid)
    {
        var userDatum = await context.UserData.FindAsync(baid);
        userDatum.ThrowIfNull($"User with baid: {baid} not found!");
        return userDatum.FavoriteSongsArray.ToList();
    }

    public async Task UpdateFavoriteSong(uint baid, uint songId, bool isFavorite)
    {
        var userDatum = await context.UserData.FindAsync(baid);
        userDatum.ThrowIfNull($"User with baid: {baid} not found!");
        
        var favoriteSet = new HashSet<uint>(userDatum.FavoriteSongsArray);
        if (isFavorite)
        {
            favoriteSet.Add(songId);
        }
        else
        {
            favoriteSet.Remove(songId);
        }

        userDatum.FavoriteSongsArray = favoriteSet.ToList();
        //logger.LogInformation("Favorite songs are: {Favorite}", userDatum.FavoriteSongsArray);
        context.Update(userDatum);
        await context.SaveChangesAsync();
    }

}