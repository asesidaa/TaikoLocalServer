namespace TaikoWebUI.Services;

/// <summary>The Song List's filters and page, kept while the user opens a song and comes back.</summary>
public sealed class SongListState
{
    public string Query { get; set; } = "";
    /// <summary>-1 = every genre.</summary>
    public int Genre { get; set; } = -1;
    public bool Favorites { get; set; }
    /// <summary>The difficulty the results filter and the stars sort refer to (1 Easy … 5 Ura Oni).</summary>
    public int Difficulty { get; set; } = 4;
    public string Status { get; set; } = "all";
    public string Sort { get; set; } = "id";
    public bool AiBattle { get; set; }
    public int Page { get; set; } = 1;

    public int ActiveFilters => new[] { Genre >= 0 || Favorites, Status != "all", Sort != "id", AiBattle, Difficulty != 4 }.Count(x => x);

    public void Reset()
    {
        Genre = -1;
        Favorites = AiBattle = false;
        Status = "all";
        Sort = "id";
        Difficulty = 4;
        Page = 1;
    }
}
