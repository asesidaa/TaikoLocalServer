using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ProfileCounterUpdater
{
    public static Ac15ProfileCounterAccess<UserSaveDataBlue> Blue { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static Ac15ProfileCounterAccess<UserSaveDataGreen> Green { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static Ac15ProfileCounterAccess<UserSaveDataYellow> Yellow { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static Ac15ProfileCounterAccess<UserSaveDataRed> Red { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static Ac15ProfileCounterAccess<UserSaveDataWhite> White { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static Ac15ProfileCounterAccess<UserSaveDataMurasaki> Murasaki { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static Ac15ProfileCounterAccess<UserSaveDataKimidori> Kimidori { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static Ac15ProfileCounterAccess<UserSaveDataMomoiro> Momoiro { get; } = new(
        Jpop: new(save => save.CategJpopCnt, (save, value) => save.CategJpopCnt = value),
        Anime: new(save => save.CategAnimeCnt, (save, value) => save.CategAnimeCnt = value),
        Vocaloid: new(save => save.CategVocaloidCnt, (save, value) => save.CategVocaloidCnt = value),
        Doyo: new(save => save.CategDoyoCnt, (save, value) => save.CategDoyoCnt = value),
        Variety: new(save => save.CategVarietyCnt, (save, value) => save.CategVarietyCnt = value),
        Classic: new(save => save.CategClassicCnt, (save, value) => save.CategClassicCnt = value),
        Game: new(save => save.CategGameCnt, (save, value) => save.CategGameCnt = value),
        Namco: new(save => save.CategNamcoCnt, (save, value) => save.CategNamcoCnt = value),
        Pushed: new(save => save.SongPushedCnt, (save, value) => save.SongPushedCnt = value),
        Favorite: new(save => save.SongFavoriteCnt, (save, value) => save.SongFavoriteCnt = value),
        Recent: new(save => save.SongRecentCnt, (save, value) => save.SongRecentCnt = value));

    public static void ApplyBlueStage(UserSaveDataBlue saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, Blue);

    public static void ApplyGreenStage(UserSaveDataGreen saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, Green);

    public static void ApplyYellowStage(UserSaveDataYellow saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, Yellow);

    public static void ApplyRedStage(UserSaveDataRed saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, Red);

    public static void ApplyWhiteStage(UserSaveDataWhite saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, White);

    public static void ApplyMurasakiStage(UserSaveDataMurasaki saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, Murasaki);

    public static void ApplyKimidoriStage(UserSaveDataKimidori saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, Kimidori);

    public static void ApplyMomoiroStage(UserSaveDataMomoiro saveData, Ac15StageResult stage)
        => ApplyStage(saveData, stage, Momoiro);

    public static void ApplyStage<TSave>(
        TSave saveData,
        Ac15StageResult stage,
        Ac15ProfileCounterAccess<TSave> counters)
    {
        if (GenreCounter(counters, stage.MusicCateg) is { } genre)
        {
            Increment(saveData, genre);
        }

        if (stage.IsPushed)
        {
            Increment(saveData, counters.Pushed);
        }

        if (stage.IsFavorite)
        {
            Increment(saveData, counters.Favorite);
        }

        if (stage.IsRecent)
        {
            Increment(saveData, counters.Recent);
        }
    }

    private static Ac15CounterAccess<TSave>? GenreCounter<TSave>(
        Ac15ProfileCounterAccess<TSave> counters,
        uint musicCateg)
        => musicCateg switch
        {
            1 => counters.Jpop,
            2 => counters.Anime,
            3 => counters.Vocaloid,
            4 => counters.Doyo,
            5 => counters.Variety,
            6 => counters.Classic,
            7 => counters.Game,
            8 => counters.Namco,
            _ => null
        };

    private static void Increment<TSave>(TSave saveData, Ac15CounterAccess<TSave> counter)
        => counter.Set(saveData, SafeIncrement(counter.Get(saveData)));

    private static uint SafeIncrement(uint current)
        => current == uint.MaxValue ? current : current + 1;
}

public sealed record Ac15ProfileCounterAccess<TSave>(
    Ac15CounterAccess<TSave> Jpop,
    Ac15CounterAccess<TSave> Anime,
    Ac15CounterAccess<TSave> Vocaloid,
    Ac15CounterAccess<TSave> Doyo,
    Ac15CounterAccess<TSave> Variety,
    Ac15CounterAccess<TSave> Classic,
    Ac15CounterAccess<TSave> Game,
    Ac15CounterAccess<TSave> Namco,
    Ac15CounterAccess<TSave> Pushed,
    Ac15CounterAccess<TSave> Favorite,
    Ac15CounterAccess<TSave> Recent);

public sealed record Ac15CounterAccess<TSave>(
    Func<TSave, uint> Get,
    Action<TSave, uint> Set);
