﻿using static MudBlazor.Colors;
using System;
using static MudBlazor.CategoryTypes;
using System.Globalization;
using Microsoft.JSInterop;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Pages;

public partial class PlayHistory
{
    [Parameter]
    public int Baid { get; set; }

    private const string IconStyle = "width:25px; height:25px;";

    private string Search { get; set; } = string.Empty;

    private string? currentLanguage;

    private SongHistoryResponse? response;

    private Dictionary<DateTime, List<SongHistoryData>> songHistoryDataMap = new();
    
    private readonly List<BreadcrumbItem> breadcrumbs = new();


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<SongHistoryResponse>($"api/PlayHistory/{(uint)Baid}");
        response.ThrowIfNull();
        
        currentLanguage = await JSRuntime.InvokeAsync<string>("blazorCulture.get");

        response.SongHistoryData.ForEach(data =>
        {
            var songId = data.SongId;
            data.Genre = GameDataService.GetMusicGenreBySongId(songId);
            data.MusicName = GameDataService.GetMusicNameBySongId(songId, string.IsNullOrEmpty(currentLanguage) ? "ja" : currentLanguage);
            data.MusicArtist = GameDataService.GetMusicArtistBySongId(songId, string.IsNullOrEmpty(currentLanguage) ? "ja" : currentLanguage);
            data.Stars = GameDataService.GetMusicStarLevel(songId, data.Difficulty);
            data.ShowDetails = false;
        });

        songHistoryDataMap = response.SongHistoryData.GroupBy(data => data.PlayTime).ToDictionary(data => data.Key,data => data.ToList());
        foreach (var songHistoryDataList in songHistoryDataMap.Values)
        {
            songHistoryDataList.Sort((data1, data2) => data1.SongNumber.CompareTo(data2.SongNumber));
        }
    }

    private static string GetCrownText(CrownType crown)
    {
        return crown switch
        {
            CrownType.None => "Fail",
            CrownType.Clear => "Clear",
            CrownType.Gold => "Full Combo",
            CrownType.Dondaful => "Donderful Combo",
            _ => ""
        };
    }

    private static string GetRankText(ScoreRank rank)
    {
        return rank switch
        {
            ScoreRank.White => "Stylish",
            ScoreRank.Bronze => "Stylish",
            ScoreRank.Silver => "Stylish",
            ScoreRank.Gold => "Graceful",
            ScoreRank.Sakura => "Graceful",
            ScoreRank.Purple => "Graceful",
            ScoreRank.Dondaful => "Top Class",
            _ => ""
        };
    }

    private static string GetDifficultyTitle(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => "Easy",
            Difficulty.Normal => "Normal",
            Difficulty.Hard => "Hard",
            Difficulty.Oni => "Oni",
            Difficulty.UraOni => "Ura Oni",
            _ => ""
        };
    }

    private static string GetDifficultyIcon(Difficulty difficulty)
    {
        return $"<image href='/images/difficulty_{difficulty}.png' alt='{difficulty}' width='24' height='24'/>";
    }

    private static string GetGenreTitle(SongGenre genre)
    {
        return genre switch
        {
            SongGenre.Pop => "Pop",
            SongGenre.Anime => "Anime",
            SongGenre.Kids => "Kids",
            SongGenre.Vocaloid => "Vocaloid",
            SongGenre.GameMusic => "Game Music",
            SongGenre.NamcoOriginal => "NAMCO Original",
            SongGenre.Variety => "Variety",
            SongGenre.Classical => "Classical",
            _ => ""
        };
    }

    private static string GetGenreStyle(SongGenre genre)
    {
        return genre switch
        {
            SongGenre.Pop => "background: #42c0d2; color: #fff",
            SongGenre.Anime => "background: #ff90d3; color: #fff",
            SongGenre.Kids => "background: #fec000; color: #fff",
            SongGenre.Vocaloid => "background: #ddd; color: #000",
            SongGenre.GameMusic => "background: #cc8aea; color: #fff",
            SongGenre.NamcoOriginal => "background: #ff7027; color: #fff",
            SongGenre.Variety => "background: #1dc83b; color: #fff",
            SongGenre.Classical => "background: #bfa356; color: #000",
            _ => ""
        };
    }
    
    private bool FilterSongs(List<SongHistoryData> songHistoryDataList)
    {
        if (string.IsNullOrWhiteSpace(Search))
        {
            return true;
        }

        var language = currentLanguage ?? "ja";
        
        if (songHistoryDataList[0].PlayTime
            .ToString("dddd d MMMM yyyy - HH:mm", CultureInfo.CreateSpecificCulture(language))
            .Contains(Search, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var songHistoryData in songHistoryDataList)
        {
            if (songHistoryData.MusicName.Contains(Search, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (songHistoryData.MusicArtist.Contains(Search, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private async Task OnFavoriteToggled(SongHistoryData data)
    {
        var request = new SetFavoriteRequest
        {
            Baid = (uint)Baid,
            IsFavorite = !data.IsFavorite,
            SongId = data.SongId
        };
        var result = await Client.PostAsJsonAsync("api/FavoriteSongs", request);
        if (result.IsSuccessStatusCode)
        {
            data.IsFavorite = !data.IsFavorite;
        }
    }
}

