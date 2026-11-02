namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// An admin's name for one of the game's attract movies (the game has none, only numbered files). Server-wide: every
/// cabinet's movie list and the defaults show it.
/// </summary>
public class MovieLabel
{
    public uint MovieId { get; set; }
    public string Name { get; set; } = string.Empty;

    public const int MaxLength = 60;
}
