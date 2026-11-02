namespace TaikoLocalServer.Application.Common;

public static class UserSaveDataNijiiroExtensions
{
    public static async ValueTask<UserSaveDataNijiiro> GetOrCreateNijiiroSaveDataAsync(
        this ITaikoDbContext context,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        var saveData = await context.UserSaveDataNijiiro.FindAsync([baid], cancellationToken);
        if (saveData is not null)
        {
            return saveData;
        }

        saveData = CreateDefaultNijiiroSaveData(baid);
        context.UserSaveDataNijiiro.Add(saveData);
        return saveData;
    }

    public static UserSaveDataNijiiro CreateDefaultNijiiroSaveData(uint baid) => new()
    {
        Baid = baid,
        DisplayDan = true,
        DisplayAchievement = true,
        AchievementDisplayDifficulty = Difficulty.None,
        ColorFace = 0,
        ColorBody = 1,
        ColorLimb = 3,
        FavoriteSongsArray = [],
        ToneFlgArray = [0],
        TitleFlgArray = [],
        UnlockedKigurumi = [0],
        UnlockedBody = [0],
        UnlockedFace = [0],
        UnlockedHead = [0],
        UnlockedPuchi = [0],
        GenericInfoFlgArray = [],
        UnlockedSongIdList = [],
        UnlockedUraSongIdList = [],
    };
}
