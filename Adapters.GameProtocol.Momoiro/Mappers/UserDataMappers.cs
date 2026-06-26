using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class UserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataProfileCounters source, [MappingTarget] UserDataResponse response);

    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DispTaikojukuDan))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedCourse))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedStar))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsChallengeCompe))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsTojiru))]
    [MapperIgnoreTarget(nameof(UserDataResponse.IsAutoTitleOn))]
    public static partial void Apply(Ac15UserDataDisplaySettings source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataModeFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataReward source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataResponse.HashCrownFlg), nameof(UserDataResponse.HashCrownFlg), Use = nameof(MapRequiredBytes))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.Result))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.SongFlags))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.SongLists))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.Counters))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.Display))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.Recommendations))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.Tutorial))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.ModeFlags))]
    [MapperIgnoreSource(nameof(Ac15UserDataResponse.Reward))]
    public static partial void ApplyCrown(Ac15UserDataResponse source, [MappingTarget] UserDataResponse response);

    private static uint[] MapRecommendBestSongs(List<uint> value) => value.ToArray();

    private static byte[] MapRequiredBytes(byte[]? value) => value ?? [];
}
