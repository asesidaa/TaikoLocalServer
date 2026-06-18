using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTaikojukuLoader
{
    public const string VerupFileName = "green_taikojuku_verup_data.json";

    public Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(
            GreenGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.Green), VerupFileName),
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
            nameof(GameEra.Green),
            cancellationToken);
}
