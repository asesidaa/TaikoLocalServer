using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class UserDataMappers
{
    [MapProperty(nameof(Ac15UserDataSongFlags.ReleaseSongFlg), nameof(UserDataResponse.HashReleaseSongFlg))]
    public static partial void Apply(Ac15UserDataSongFlags source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataSongLists source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataRecommendations.RecommendBestSong), nameof(UserDataResponse.RecommendBestSongs), Use = nameof(MapRecommendBestSongs))]
    public static partial void Apply(Ac15UserDataRecommendations source, [MappingTarget] UserDataResponse response);

    public static partial void Apply(Ac15UserDataProfileCounters source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataDisplaySettings.DispTaikojukuDan), nameof(UserDataResponse.DispTaikojukuDan), Use = nameof(MapDispTaikojukuDan))]
    [MapProperty(nameof(Ac15UserDataDisplaySettings.IsChallengeCompe), nameof(UserDataResponse.IsChallengecompe))]
    public static partial void Apply(Ac15UserDataDisplaySettings source, [MappingTarget] UserDataResponse response);

    [MapProperty(nameof(Ac15UserDataModeFlags.IsExplain), nameof(UserDataResponse.IsExplain))]
    public static partial void Apply(Ac15UserDataModeFlags source, [MappingTarget] UserDataResponse response);

    private static uint? MapDispTaikojukuDan(uint value)
    {
        // disp_taikojuku_dan MUST be 1..25 on the wire. Omitting it does not
        // help: the Green client reads disp_taikojuku_dan_ at message +0x31C
        // without checking proto2 presence (sub_19CFE0:151, sub_1016F8:506,
        // sub_24377C:176, sub_7FDFFC:755). Any out-of-range value - including
        // 0 from an absent tag - underflows Taikojuku_GetDanSlotSongRange's
        // 84-byte-per-slot table at 0x127F98 and crashes the client at boot.
        // sub_7FDFFC uses 1 as its baked-in "no data" default; mirror that.
        return value is >= 1 and <= 25 ? value : 1u;
    }

    private static uint[] MapRecommendBestSongs(List<uint> value) => value.ToArray();
}
