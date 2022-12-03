using System.Text.Json;
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
        return await context.UserData.FindAsync(baid);
    }

    public async Task<UserDatum> GetFirstUserDatumOrDefault(uint baid)
    {
        return await context.UserData.FindAsync(baid) ?? new UserDatum();
    }

    public async Task<List<UserDatum>> GetAllUserData()
    {
        return await context.UserData.ToListAsync();
    }

    public async Task UpdateOrInsertUserDatum(UserDatum userDatum)
    {
        if (await context.UserData.AnyAsync(datum => datum.Baid == userDatum.Baid))
        {
            context.UserData.Add(userDatum);
            await context.SaveChangesAsync();
            return;
        }

        context.Update(userDatum);
        await context.SaveChangesAsync();
    }

    public async Task InsertUserDatum(UserDatum userDatum)
    {
        context.UserData.Add(userDatum);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserDatum(UserDatum userDatum)
    {
        context.Update(userDatum);
        await context.SaveChangesAsync();
    }

    public async Task<List<uint>> GetFavoriteSongIds(uint baid)
    {
        var userDatum = await context.UserData.FindAsync(baid);
        userDatum.ThrowIfNull($"User with baid: {baid} not found!");

        using var stringStream = GZipBytesUtil.GenerateStreamFromString(userDatum.FavoriteSongsArray);
        List<uint>? result;
        try
        {
            result = await JsonSerializer.DeserializeAsync<List<uint>>(stringStream);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parse favorite song array json failed! Is the user initialized correctly?");
            result = new List<uint>();
        }

        result.ThrowIfNull("Song favorite array should never be null!");
        return result;
    }

    public async Task UpdateFavoriteSong(uint baid, uint songId, bool isFavorite)
    {
        var userDatum = await context.UserData.FindAsync(baid);
        userDatum.ThrowIfNull($"User with baid: {baid} not found!");

        using var stringStream = GZipBytesUtil.GenerateStreamFromString(userDatum.FavoriteSongsArray);
        List<uint>? favoriteSongIds;
        try
        {
            favoriteSongIds = await JsonSerializer.DeserializeAsync<List<uint>>(stringStream);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parse favorite song array json failed! Is the user initialized correctly?");
            favoriteSongIds = new List<uint>();
        }

        favoriteSongIds.ThrowIfNull("Song favorite array should never be null!");
        var favoriteSet = new HashSet<uint>(favoriteSongIds);
        if (isFavorite)
            favoriteSet.Add(songId);
        else
            favoriteSet.Remove(songId);

        using var newFavoriteSongStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(newFavoriteSongStream, favoriteSet);
        newFavoriteSongStream.Position = 0;
        using var reader = new StreamReader(newFavoriteSongStream);

        userDatum.FavoriteSongsArray = await reader.ReadToEndAsync();
        logger.LogInformation("Favorite songs are: {Favorite}", userDatum.FavoriteSongsArray);
        context.Update(userDatum);
        await context.SaveChangesAsync();
    }
}