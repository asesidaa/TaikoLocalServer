using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class UserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(@Ac15MapperNormalization.ToUIntArray))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataProfileCounters source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataDisplaySettings.DispTaikojukuDan), nameof(UserDataResponse.DispTaikojukuDan), Use = nameof(MapDispTaikojukuDan))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedCourse))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedStar))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsChallengeCompe))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsTojiru))]
    public static partial void Apply(Ac15UserDataDisplaySettings source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataModeFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataReward source, [MappingTarget] UserDataResponse response);

    private static uint? MapDispTaikojukuDan(uint value)
        => Ac15MapperNormalization.DisplayDan(value, Ac15EraProfiles.Murasaki.Limits);
}
