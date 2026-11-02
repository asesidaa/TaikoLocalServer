using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Kimidori;

internal static class KimidoriHandlerFixture
{
    internal sealed class TestKimidoriCatalog : IKimidoriCatalog
    {
        private readonly IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder;

        public TestKimidoriCatalog(IReadOnlyList<Ac15MusicInfoEntry>? musicInfoFileOrder = null)
        {
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
        }

        public GameEra Era => GameEra.Kimidori;

        public uint SongHashVersion => 505;

        public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyList<ushort> SongHashTable
            => MusicInfoFileOrder.Select(song => checked((ushort)song.SongNo)).ToArray();

        public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> KimidoriMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder { get; init; } = [];

        public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder { get; init; } = [];

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; init; } =
            new Dictionary<uint, EventFolderData>();

        public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; init; } =
            new Dictionary<uint, Ac15TelopEntry>();

        public IReadOnlyList<MovieData> Movies { get; init; } = [];

        public IReadOnlyList<Ac15PresentItem> Presents { get; init; } = [];

        public IReadOnlyList<Ac15SpecialBaidEntry> SpecialBaids { get; init; } = [];

        public IReadOnlyList<Costume> CostumeList { get; init; } =
        [
            new() { CostumeId = 0, CostumeType = "kigurumi" },
            new() { CostumeId = 1, CostumeType = "head" },
            new() { CostumeId = 2, CostumeType = "body" },
            new() { CostumeId = 3, CostumeType = "face" },
            new() { CostumeId = 4, CostumeType = "puchi" }
        ];

        public IReadOnlyDictionary<uint, Title> TitleDictionary { get; init; } =
            new Dictionary<uint, Title>
            {
                [10] = new() { TitleId = 10, TitleName = "Kimidori Title", TitleRarity = 0 }
            };

        public IReadOnlyDictionary<uint, Neiro> NeiroDictionary { get; init; } =
            new Dictionary<uint, Neiro>
            {
                [0] = new() { NeiroId = 0, NeiroName = "Taiko" },
                [4] = new() { NeiroId = 4, NeiroName = "Tone 4" }
            };

        public IReadOnlyList<Costume> GetCostumeList() => CostumeList;

        public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => TitleDictionary;

        public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => NeiroDictionary;

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static IReadOnlyList<Ac15MusicInfoEntry> DefaultMusicInfoFileOrder { get; } =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 }
        ];
    }
}
