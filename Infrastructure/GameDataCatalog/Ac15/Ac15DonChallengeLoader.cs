using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema;
using TaikoLocalServer.Application.Ac15.DonChallenge;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15DonChallengeLoader
{
    private const string SchemaResourceName =
        "TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15.Schemas.ac15-don-challenge-catalog.schema.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters =
        {
            new JsonStringEnumConverter<Ac15DonChallengeRuleKind>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)
        }
    };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly EvaluationOptions SchemaEvaluationOptions = new()
    {
        OutputFormat = OutputFormat.List
    };

    private static readonly JsonDocument SchemaDocument = LoadSchemaDocument();

    private static readonly JsonSchema DataSchema = JsonSchema.Build(SchemaDocument.RootElement);

    public static async Task<Ac15DonChallengeCatalog> LoadFromFileAsync(
        string path,
        bool isEnabled,
        string? activeBundleId,
        string eraName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!isEnabled)
        {
            return Ac15DonChallengeCatalog.Disabled;
        }

        if (!File.Exists(path))
        {
            throw new InvalidDataException($"{eraName} Don Challenge is enabled but data file was not found: {path}");
        }

        using var document = await ParseDocumentAsync(path, eraName, cancellationToken);
        ValidateSchema(document.RootElement, path, eraName);

        Ac15DonChallengeCatalog catalog;
        try
        {
            catalog = document.RootElement.Deserialize<Ac15DonChallengeCatalog>(JsonOptions)
                      ?? Ac15DonChallengeCatalog.Disabled;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"{eraName} Don Challenge data is malformed: {path}", ex);
        }

        if (!catalog.Enabled)
        {
            return Ac15DonChallengeCatalog.Disabled;
        }

        catalog = catalog with { ActiveBundleId = activeBundleId?.Trim() };
        ValidateCatalogSemantics(catalog, eraName);
        return catalog;
    }

    private static async Task<JsonDocument> ParseDocumentAsync(
        string path,
        string eraName,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonDocument.ParseAsync(stream, DocumentOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"{eraName} Don Challenge data is malformed: {path}", ex);
        }
    }

    private static void ValidateSchema(JsonElement root, string path, string eraName)
    {
        var results = DataSchema.Evaluate(root, SchemaEvaluationOptions);
        if (results.IsValid)
        {
            return;
        }

        var errors = CollectErrors(results)
            .Take(10)
            .ToArray();
        throw new InvalidDataException(
            $"{eraName} Don Challenge data failed schema validation: {path}. {string.Join("; ", errors)}");
    }

    private static IEnumerable<string> CollectErrors(EvaluationResults results)
    {
        if (results.Errors is { Count: > 0 })
        {
            foreach (var error in results.Errors)
            {
                yield return $"{results.InstanceLocation}: {error.Value}";
            }
        }

        foreach (var detail in results.Details ?? [])
        {
            foreach (var error in CollectErrors(detail))
            {
                yield return error;
            }
        }
    }

    private static void ValidateCatalogSemantics(Ac15DonChallengeCatalog catalog, string eraName)
    {
        if (string.IsNullOrWhiteSpace(catalog.ActiveBundleId))
        {
            throw new InvalidDataException($"{eraName} Don Challenge is enabled but ActiveDonChallengeBundleId is not configured.");
        }

        var duplicateBundleIds = catalog.MonthlyBundles
            .GroupBy(bundle => bundle.BundleId, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicateBundleIds.Length > 0)
        {
            throw new InvalidDataException($"{eraName} Don Challenge data contains duplicate bundle_id values: {string.Join(", ", duplicateBundleIds)}");
        }

        if (catalog.ActiveBundle is null)
        {
            throw new InvalidDataException($"{eraName} Don Challenge active bundle {catalog.ActiveBundleId} was not found.");
        }

        foreach (var bundle in catalog.MonthlyBundles)
        {
            var duplicateSlots = bundle.PersonalTasks
                .GroupBy(task => task.Slot)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            if (duplicateSlots.Length > 0)
            {
                throw new InvalidDataException($"{eraName} Don Challenge bundle {bundle.BundleId} contains duplicate personal task slots: {string.Join(", ", duplicateSlots)}");
            }
        }
    }

    private static JsonDocument LoadSchemaDocument()
    {
        using var stream = typeof(Ac15DonChallengeLoader).Assembly.GetManifestResourceStream(SchemaResourceName)
                           ?? throw new InvalidOperationException($"Embedded Don Challenge schema resource was not found: {SchemaResourceName}");
        return JsonDocument.Parse(stream);
    }
}
