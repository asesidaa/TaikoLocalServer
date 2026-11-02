namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public sealed record Ac15CustomizationCatalog(
    IReadOnlyList<Costume> Costumes,
    IReadOnlyDictionary<uint, Title> Titles,
    IReadOnlyDictionary<uint, Neiro> Neiros);
