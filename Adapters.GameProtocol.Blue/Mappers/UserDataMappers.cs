using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class UserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(@Ac15MapperNormalization.ToUIntArray))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataProfileCounters source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataDisplaySettings.DispTaikojukuDan), nameof(UserDataResponse.DispTaikojukuDan), Use = nameof(NormalizeDispTaikojukuDan))]
    [MapProperty(nameof(Ac15UserDataDisplaySettings.IsChallengeCompe), nameof(UserDataResponse.IsChallengecompe))]
    public static partial void Apply(Ac15UserDataDisplaySettings source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataModeFlags.IsExplain), nameof(UserDataResponse.IsExplain))]
    public static partial void Apply(Ac15UserDataModeFlags source, [MappingTarget] UserDataResponse response);

    [MapperIgnoreSource(nameof(Ac15UserDataTutorial.DifficultyTutorialFlg))]
    public static partial void Apply(Ac15UserDataTutorial source, [MappingTarget] UserDataResponse response);

    private static uint? NormalizeDispTaikojukuDan(uint value)
        => Ac15MapperNormalization.DisplayDan(value, Ac15EraProfiles.Blue.Limits);
}
