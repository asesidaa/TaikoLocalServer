using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueTaikojukuLoader
{
    public const string VerupFileName = "blue_taikojuku_verup_data.json";

    public Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(
            BlueGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.Blue), VerupFileName),
            cancellationToken);

    public static Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15TaikojukuLoader.LoadFromFileAsync(path, cancellationToken);

    public static Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadFromFileAsync(
        string path,
        string verupPath,
        CancellationToken cancellationToken)
        => Ac15TaikojukuLoader.LoadFromFileAsync(
            path,
            verupPath,
            nameof(GameEra.Blue),
            cancellationToken);
}
