using System.Text.Json;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>The game's attract movies and the names admins give them (Server Management › Cabinets). Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class MoviesController(ITaikoDbContext context) : BaseAdminController<MoviesController>
{
    public record NameRequest(string Name);
    private sealed record MovieIndex(List<uint> Movies);

    // Written by extract: { "movies": [1, 2, …] }.
    private static string IndexPath => Path.Combine(PathHelper.GetRootPath(), PathHelper.GameDataFolder, "movies.json");

    [HttpGet]
    public async Task<GameMovies> Get()
    {
        List<uint>? inGame = null;
        if (System.IO.File.Exists(IndexPath))
        {
            await using var file = System.IO.File.OpenRead(IndexPath);
            inGame = (await JsonSerializer.DeserializeAsync<MovieIndex>(file, new JsonSerializerOptions(JsonSerializerDefaults.Web), HttpContext.RequestAborted))?.Movies
                .Distinct().Order().ToList();
        }
        var names = await context.MovieLabels.ToDictionaryAsync(l => l.MovieId, l => l.Name, HttpContext.RequestAborted);
        return new GameMovies(inGame, names);
    }

    /// <summary>Names a movie for every cabinet; an empty name removes it.</summary>
    [HttpPut("{id}/Name")]
    public async Task<IActionResult> SetName(uint id, NameRequest request)
    {
        var name = request.Name.Trim();
        if (name.Length > MovieLabel.MaxLength) return BadRequest(new { message = $"At most {MovieLabel.MaxLength} characters." });
        var label = await context.MovieLabels.FindAsync([id], HttpContext.RequestAborted);
        if (name.Length == 0)
        {
            if (label is not null) context.MovieLabels.Remove(label);
        }
        else if (label is null) context.MovieLabels.Add(new MovieLabel { MovieId = id, Name = name });
        else label.Name = name;
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
