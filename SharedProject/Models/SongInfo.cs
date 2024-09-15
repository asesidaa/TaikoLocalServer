using SharedProject.Enums;

namespace SharedProject.Models;

public class SongInfo
{
    public MusicDetail MusicDetail { get; set; } = new();

    public Difficulty Difficulty { get; set; } = new();

}
