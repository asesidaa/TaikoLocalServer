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

        logger.LogWarning("{FieldName} is null or length less than {Length}!", fieldName, length);
        array = new uint[length];

        return array;
    }
}