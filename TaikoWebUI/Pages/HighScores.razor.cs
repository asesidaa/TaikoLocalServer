﻿using static MudBlazor.Colors;
using System;
using Microsoft.JSInterop;


namespace TaikoWebUI.Pages;

public partial class HighScores
{
    [Parameter]
    public int Baid { get; set; }

    private SongBestResponse? response;
    private UserSetting? userSetting;
    private Dictionary<Difficulty, List<SongBestData>> songBestDataMap = new();

    private readonly List<BreadcrumbItem> breadcrumbs = new();
    private int selectedDifficultyTab = 0;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}");
        response.ThrowIfNull();

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        var language = await JsRuntime.InvokeAsync<string>("blazorCulture.get");

        response.SongBestData.ForEach(data =>
        {
            var songId = data.SongId;
            data.Genre = GameDataService.GetMusicGenreBySongId(songId);
            data.MusicName = GameDataService.GetMusicNameBySongId(songId, string.IsNullOrEmpty(language) ? "ja" : language);
            data.MusicArtist = GameDataService.GetMusicArtistBySongId(songId, string.IsNullOrEmpty(language) ? "ja" : language);
        });

        songBestDataMap = response.SongBestData.GroupBy(data => data.Difficulty)
            .ToDictionary(data => data.Key,
                          data => data.ToList());
        foreach (var songBestDataList in songBestDataMap.Values)
        {
            songBestDataList.Sort((data1, data2) => GameDataService.GetMusicIndexBySongId(data1.SongId)
                                      .CompareTo(GameDataService.GetMusicIndexBySongId(data2.SongId)));
        }
        
        // Set last selected tab from local storage
        selectedDifficultyTab = await LocalStorage.GetItemAsync<int>($"highScoresTab");

        // Breadcrumbs
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin)
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        }
        else
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        };
        breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem(Localizer["High Scores"], href: $"/Users/{Baid}/HighScores", disabled: false));
    }

    private async Task OnFavoriteToggled(SongBestData data)
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

    private async Task OnTabChanged(int index)
    {
        selectedDifficultyTab = index;
        await LocalStorage.SetItemAsync($"highScoresTab", selectedDifficultyTab);
    }
}
