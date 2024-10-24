using GameDatabase.Context;
using Throw;
using SharedProject.Models;
using MudExtensions;

namespace TaikoLocalServer.Services;

public class UserDatumService(TaikoDbContext context) : IUserDatumService
{
    public async Task<List<UserDatum>> GetAllUserDatum()
    {
        return await context.UserData.Include(d => d.Tokens).ToListAsync();
    }

    public async Task<Dictionary<uint, User>> GetAllUserDict()
    {
        List<UserDatum> listUser = await GetAllUserDatum();
        Dictionary<uint, User> dict = new Dictionary<uint, User>();
        foreach (var user in listUser)
        {
            dict.Add(user.Baid, new()
            {
                Baid = user.Baid,
                IsAdmin = false,
                UserSetting = new()
                {
                    Baid = user.Baid,
                    ToneId = user.SelectedToneId,
                    IsDisplayAchievement = user.DisplayAchievement,
                    IsDisplayDanOnNamePlate = user.DisplayDan,
                    DifficultySettingCourse = user.DifficultySettingCourse,
                    DifficultySettingStar = user.DifficultyPlayedStar,
                    DifficultySettingSort = user.DifficultyPlayedSort,
                    IsVoiceOn = user.IsVoiceOn,
                    IsSkipOn = user.IsSkipOn,
                    AchievementDisplayDifficulty = user.AchievementDisplayDifficulty,
                    MyDonName = user.MyDonName,
                    MyDonNameLanguage = user.MyDonNameLanguage,
                    Title = user.Title,
                    TitlePlateId = user.TitlePlateId,
                    Kigurumi = user.CurrentKigurumi,
                    Head = user.CurrentHead,
                    Body = user.CurrentBody,
                    Face = user.CurrentFace,
                    Puchi = user.CurrentPuchi,
                    FaceColor = user.ColorFace,
                    BodyColor = user.ColorFace,
                    LimbColor = user.ColorLimb
                }
            });
        }
        return dict;
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