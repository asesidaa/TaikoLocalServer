using System.Net.Http.Json;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Domain.Enums;
using TaikoWebUI.Services;

namespace TaikoWebUI.Components.Eras;

/// <summary>
/// One player's results in an older game (api/{game}/PlayData: a row per chart with its plays) and that game's songs,
/// for its Song List, High Scores, Play History and Play Data index.
/// </summary>
public sealed record EraScores(IReadOnlyList<SongBestData> Bests, IReadOnlyDictionary<uint, MusicDetail> Songs, uint SongLanguage = 0)
{
    public static async Task<EraScores> LoadAsync(HttpClient client, GameDataService gameData, string game, long baid)
    {
        var bests = client.GetFromJsonAsync<SongBestResponse>($"api/{game}/PlayData/{baid}");
        var songs = gameData.GetMusicDetailDictionary(game);
        return new((await bests)?.SongBestData ?? [], await songs, gameData.SongLanguage);
    }

    public string Name(uint songId) =>
        Songs.TryGetValue(songId, out var s) && s.Title(SongLanguage) is { Length: > 0 } title ? title : $"#{songId}";

    public IEnumerable<SongPlayDatumDto> Plays => Bests.SelectMany(b => b.RecentPlayData);

    public int Clears => Bests.Count(b => b.BestCrown >= CrownType.Clear);

    public int FullCombos => Bests.Count(b => b.BestCrown >= CrownType.Gold);
}
