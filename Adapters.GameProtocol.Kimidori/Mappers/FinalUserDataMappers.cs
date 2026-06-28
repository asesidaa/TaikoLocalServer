using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;
using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class FinalUserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(FinalWire.UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] FinalWire.UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] FinalWire.UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(FinalWire.UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] FinalWire.UserDataResponse response);

    public static partial void Apply(Ac15UserDataProfileCounters source, [MappingTarget] FinalWire.UserDataResponse response);

    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedCourse))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.DifficultyPlayedStar))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsChallengeCompe))]
    [MapperIgnoreSource(nameof(Ac15UserDataDisplaySettings.IsTojiru))]
    public static partial void Apply(Ac15UserDataDisplaySettings source, [MappingTarget] FinalWire.UserDataResponse response);

    public static partial void Apply(Ac15UserDataModeFlags source, [MappingTarget] FinalWire.UserDataResponse response);

    public static partial void Apply(Ac15UserDataReward source, [MappingTarget] FinalWire.UserDataResponse response);

    private static uint[] MapRecommendBestSongs(List<uint> value) => value.ToArray();
}
