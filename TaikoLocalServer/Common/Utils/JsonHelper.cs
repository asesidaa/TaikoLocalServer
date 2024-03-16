using GameDatabase.Entities;
using System.Text.Json;

namespace TaikoLocalServer.Common.Utils;

public static class JsonHelper
{
    public static uint[] GetUIntArrayFromJson(string data, int length, ILogger logger, string fieldName)
    {
        var array = new uint[length];
        try
        {
            array = JsonSerializer.Deserialize<uint[]>(data);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parsing {FieldName} json data failed", fieldName);
        }

        if (array != null && array.Length >= length)
        {
            return array;
        }

        logger.LogWarning($"{fieldName} is null or length less than {length}!");
        array = new uint[length];

        return array;
    }

    public static List<uint> GetCostumeDataFromUserData(UserDatum userData, ILogger logger)
    {
        var costumeData = new List<uint> { 0, 0, 0, 0, 0 };
        try
        {
            // logger.LogInformation(userData.CostumeData);
            costumeData = JsonSerializer.Deserialize<List<uint>>(userData.CostumeData);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parsing costume json data failed");
        }

        if (costumeData != null && costumeData.Count >= 5)
        {
            return costumeData;
        }

        logger.LogWarning("Costume data is null or count less than 5!");
        costumeData = new List<uint> { 0, 0, 0, 0, 0 };

        return costumeData;
    }
}