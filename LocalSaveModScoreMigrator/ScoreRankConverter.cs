using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharedProject.Enums;

namespace LocalSaveModScoreMigrator;

public class ScoreRankConverter : JsonConverter<ScoreRank>
{
    public override ScoreRank Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(ScoreRank));
        var scoreRankInt = reader.GetInt32() + 1;
        return (ScoreRank)scoreRankInt;
    }

    public override void Write(Utf8JsonWriter writer, ScoreRank value, JsonSerializerOptions options)
    {
        var scoreRank = (int)value - 1;
        writer.WriteStringValue(scoreRank.ToString());
    }
}