using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using System.Text.Json;
using GameDatabase.Context;
using GameDatabase.Entities;
using ICSharpCode.SharpZipLib.GZip;
using JorgeSerrano.Json;
using LocalSaveModScoreMigrator;
using SharedProject.Enums;

var rootCommand = new RootCommand("Command-line tool to migrate saves from local save mod to local server database.");

FileInfo? Parse(SymbolResult result, string defaultFileName)
{
    if (result.Tokens.Count == 0)
    {
        return new FileInfo(defaultFileName);
    }

    var filePath = result.Tokens.Single().Value;
    if (File.Exists(filePath))
    {
        return new FileInfo(filePath);
    }

    result.ErrorMessage = $"File {filePath} does not exist";
    return null;
}

var saveFileArgument = new Argument<FileInfo?>(
    name: "--save-file",
    description: "Path to the save file from local save mod",
    isDefault: true,
    parse: result => Parse(result, "record_enso_p1.json")
);

var dbFileArgument = new Argument<FileInfo?>(
    name: "--db-file-path",
    description: "Path to the database file for local server",
    isDefault: true,
    parse: result => Parse(result, "wwwroot/taiko.db3")
);

var musicInfoArgument = new Argument<FileInfo?>(
    name: "--music-info-file",
    description: "Path to the music info json/bin file",
    isDefault: true,
    parse: result => Parse(result, "wwwroot/data/musicinfo.json")
);

var baidArgument = new Argument<int>(
    name: "--baid",
    description: "Target card's baid, data will be imported to that card",
    getDefaultValue: () => 1
);

rootCommand.Add(saveFileArgument);
rootCommand.Add(dbFileArgument);
rootCommand.Add(musicInfoArgument);
rootCommand.Add(baidArgument);

rootCommand.SetHandler((saveFile, dbFile, musicInfoFile, baid) => Run(saveFile!, dbFile!, musicInfoFile!, baid), 
    saveFileArgument, dbFileArgument, musicInfoArgument, baidArgument);

await rootCommand.InvokeAsync(args);

void Run(FileSystemInfo saveFile, FileSystemInfo dbFile, FileSystemInfo musicInfoFile, int baid)
{
    using var db = new TaikoDbContext(dbFile.FullName);
    var card = db.Cards.First(card1 => card1.Baid == baid);
    Console.WriteLine(card.Baid);
    Console.WriteLine(card.AccessCode);

    var localSaveJson = File.ReadAllText(saveFile.FullName);
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
    
    var musicInfoJson = File.ReadAllText(musicInfoFile.FullName);
    if (musicInfoFile.FullName.EndsWith(".bin"))
    {
        var compressed = File.OpenRead(musicInfoFile.FullName);
        using var gZipInputStream = new GZipInputStream(compressed);
        using var decompressed = new MemoryStream();
            
        // Decompress
        gZipInputStream.CopyTo(decompressed);

        // Reset stream for reading
        decompressed.Position = 0;
        musicInfoJson = Encoding.UTF8.GetString(decompressed.ToArray());
    }
    var musicInfo = JsonSerializer.Deserialize<MusicInfo>(musicInfoJson);

    if (musicInfo is null)
    {
        throw new ApplicationException("Music info is null");
    }

    var user = db.UserData.First();
    var musicInfoMap = musicInfo.Items.DistinctBy(entry => entry.Id)
        .ToDictionary(entry => entry.Id, entry => entry.SongId);
    foreach (var playRecord in playRecordJson)
    {
        var key = playRecord.SongId.Split("_")[1];
        if (!musicInfoMap.ContainsKey(key))
        {
            Console.WriteLine($"Key {key} does not exist!!!");
            continue;
        }
        var songId = musicInfoMap[key];
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
}