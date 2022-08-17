using System.Text.Json;
using TaikoLocalServer.Models;

namespace TaikoLocalServer.Utils;

public class MusicAttributeManager
{
    private Dictionary<uint,MusicAttribute> musicAttributes;

    static MusicAttributeManager()
    {
    }

    private MusicAttributeManager()
    {
        var path = Environment.ProcessPath;
        if (path is null)
        {
            throw new ApplicationException();
        }
        var parentPath = Directory.GetParent(path);
        if (parentPath is null)
        {
            throw new ApplicationException();
        }
        var filePath = Path.Combine(parentPath.ToString(), "wwwroot", "music_attribute.json");
        var jsonString = File.ReadAllText(filePath);
        var result = JsonSerializer.Deserialize<List<MusicAttribute>>(jsonString);
        if (result is null)
        {
            throw new ApplicationException();
        }
        musicAttributes = result.ToDictionary(attribute => attribute.MusicId);
    }

    public static MusicAttributeManager Instance { get; } = new();

    public bool MusicHasUra(uint musicId)
    {
        return musicAttributes[musicId].HasUra;
    }
}