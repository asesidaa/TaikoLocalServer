namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed record Ac15MusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<Ac15MusicInfoEntry> Entries);
