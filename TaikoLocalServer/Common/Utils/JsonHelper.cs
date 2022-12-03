using System.Text.Json;
using GameDatabase.Entities;

namespace TaikoLocalServer.Common.Utils;

public static class JsonHelper
{
    public static List<uint> GetCostumeDataFromUserData(UserDatum userData, ILogger logger)
    {
        var costumeData = new List<uint> { 0, 0, 0, 0, 0 };
        try
        {
            costumeData = JsonSerializer.Deserialize<List<uint>>(userData.CostumeData);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parsing costume json data failed");
        }

        if (costumeData != null && costumeData.Count >= 5) return costumeData;

        logger.LogWarning("Costume data is null or count less than 5!");
        costumeData = new List<uint> { 0, 0, 0, 0, 0 };

        return costumeData;
    }

    public static List<List<uint>> GetCostumeUnlockDataFromUserData(UserDatum userData, ILogger logger)
    {
        var costumeUnlockData = new List<List<uint>> { new(), new(), new(), new(), new() };
        try
        {
            costumeUnlockData = JsonSerializer.Deserialize<List<List<uint>>>(userData.CostumeFlgArray);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parsing costume json data failed");
        }

        if (costumeUnlockData != null && costumeUnlockData.Count >= 5) return costumeUnlockData;

        logger.LogWarning("Costume unlock data is null or count less than 5!");
        costumeUnlockData = new List<List<uint>> { new(), new(), new(), new(), new() };

        return costumeUnlockData;
    }
}