using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15MovieLoader
{
    private const uint DefaultEnableDays = 999;

    private static readonly Regex MovieFileRegex =
        new(@"^attract_cm_(\d{3})\.pam$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task<IReadOnlyList<MovieData>> LoadFromFileAsync(
        string configPath,
        string movieDirectory,
        string eraName,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var discoveredMovieIds = DiscoverMovieIds(movieDirectory, eraName, logger);
        if (!File.Exists(configPath))
        {
            return CreateDefaultMovies(discoveredMovieIds);
        }

        Ac15MovieConfig? config;
        try
        {
            await using var stream = File.OpenRead(configPath);
            config = await JsonSerializer.DeserializeAsync<Ac15MovieConfig>(
                stream,
                JsonOptions,
                cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Invalid {eraName} movie data JSON: {configPath}", ex);
        }

        if (config?.OverrideDefault is null)
        {
            throw new InvalidDataException(
                $"{eraName} movie data must include boolean property override_default: {configPath}");
        }

        if (!config.OverrideDefault.Value)
        {
            return CreateDefaultMovies(discoveredMovieIds);
        }

        return CreateOverrideMovies(configPath, config.Movies ?? [], discoveredMovieIds, eraName, logger);
    }

    private static IReadOnlySet<uint> DiscoverMovieIds(string movieDirectory, string eraName, ILogger logger)
    {
        if (!Directory.Exists(movieDirectory))
        {
            logger.LogWarning("{EraName} movie directory does not exist: {MovieDirectory}", eraName, movieDirectory);
            return new HashSet<uint>();
        }

        var result = new HashSet<uint>();
        foreach (var path in Directory.EnumerateFiles(movieDirectory, "attract_cm_*.pam"))
        {
            var match = MovieFileRegex.Match(Path.GetFileName(path));
            if (!match.Success)
            {
                continue;
            }

            var movieId = uint.Parse(match.Groups[1].Value);
            if (movieId != 0)
            {
                result.Add(movieId);
            }
        }

        return result;
    }

    private static IReadOnlyList<MovieData> CreateDefaultMovies(IReadOnlySet<uint> discoveredMovieIds)
    {
        return discoveredMovieIds
            .Order()
            .Select(movieId => new MovieData
            {
                MovieId = movieId,
                EnableDays = DefaultEnableDays
            })
            .ToArray();
    }

    private static IReadOnlyList<MovieData> CreateOverrideMovies(
        string configPath,
        IReadOnlyList<MovieData> configuredMovies,
        IReadOnlySet<uint> discoveredMovieIds,
        string eraName,
        ILogger logger)
    {
        var duplicateIds = configuredMovies
            .Where(movie => movie.MovieId != 0)
            .GroupBy(movie => movie.MovieId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order()
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new InvalidDataException(
                $"Duplicate {eraName} movie IDs in {configPath}: {string.Join(", ", duplicateIds)}");
        }

        var result = new List<MovieData>();
        foreach (var movie in configuredMovies)
        {
            if (movie.MovieId == 0)
            {
                continue;
            }

            if (!discoveredMovieIds.Contains(movie.MovieId))
            {
                logger.LogWarning(
                    "{EraName} movie config references missing attract movie ID {MovieId}; skipping.",
                    eraName,
                    movie.MovieId);
                continue;
            }

            result.Add(new MovieData
            {
                MovieId = movie.MovieId,
                EnableDays = movie.EnableDays
            });
        }

        return result
            .OrderBy(movie => movie.MovieId)
            .ToArray();
    }

    private sealed class Ac15MovieConfig
    {
        [JsonPropertyName("override_default")]
        public bool? OverrideDefault { get; set; }

        [JsonPropertyName("movies")]
        public MovieData[]? Movies { get; set; }
    }
}
