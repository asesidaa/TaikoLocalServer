// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using GameDatabase.Context;
using GameDatabase.Entities;
using JorgeSerrano.Json;
using LocalSaveModScoreMigrator;
using SharedProject.Enums;

using var db = new TaikoDbContext();

var card = db.Cards.First();
Console.WriteLine(card.Baid);
Console.WriteLine(card.AccessCode);

var localSaveJson = File.ReadAllText("record_enso_p1.json");
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
};
options.Converters.Add(new DateTimeConverter());
options.Converters.Add(new ScoreRankConverter());
var playRecordJson = JsonSerializer.Deserialize<List<PlayRecordJson>>(localSaveJson, options);
if (playRecordJson is null)
{
    throw new ApplicationException("Play record json is null");
}

Console.WriteLine(playRecordJson.First().SongId);

var musicInfoJson = File.ReadAllText("musicinfo.json");
var musicInfo = JsonSerializer.Deserialize<MusicInfo>(musicInfoJson);

if (musicInfo is null)
{
    throw new ApplicationException("Music info is null");
}

var user = db.UserData.First();
var musicInfoMap = musicInfo.Items.DistinctBy(entry => entry.Id).ToDictionary(entry => entry.Id, entry => entry.SongId);
foreach (var playRecord in playRecordJson)
{
    var songId = musicInfoMap[playRecord.SongId.Split("_")[1]];
    Console.WriteLine(songId);
    Console.WriteLine(playRecord.DateTime);
    var playLog = new SongPlayDatum
    {
        Baid = user.Baid,
        Difficulty = playRecord.Difficulty,
        Crown = playRecord.Crown,
        Score = playRecord.Score,
        ScoreRank = playRecord.Scorerank,
        ComboCount = playRecord.Combo,
        DrumrollCount = playRecord.Drumroll,
        PlayTime = playRecord.DateTime,
        GoodCount = playRecord.Good,
        MissCount = playRecord.Bad,
        OkCount = playRecord.Ok,
        Skipped = false,
        SongNumber = 0,
        SongId = songId
    };
    db.SongPlayData.Add(playLog);


    var best = new SongBestDatum
    {
        Baid = user.Baid,
        Difficulty = playRecord.Difficulty,
        BestCrown = playRecord.Crown,
        BestScore = playRecord.Score,
        BestScoreRank = playRecord.Scorerank,
        SongId = songId
    };

    var existing = db.SongBestData.FirstOrDefault(datum => datum.Baid == user.Baid &&
                                                           datum.Difficulty == playLog.Difficulty &&
                                                           datum.SongId == songId);


    if (existing is null)
    {
        db.SongBestData.Add(best);
    }
    else
    {
        existing.BestCrown = (CrownType)Math.Max((int)existing.BestCrown, (int)playRecord.Crown);
        existing.BestScoreRank = (ScoreRank)Math.Max((int)existing.BestScoreRank, (int)playRecord.Scorerank);
        existing.BestScore = Math.Max(existing.BestScore, playRecord.Score);
        db.SongBestData.Update(existing);
    }
}

db.SaveChanges();