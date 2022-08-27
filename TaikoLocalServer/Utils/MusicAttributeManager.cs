using System.Text.Json;

namespace TaikoLocalServer.Utils;

public class MusicAttributeManager
{
    public readonly Dictionary<uint,MusicAttribute> MusicAttributes;

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
        MusicAttributes = result.ToDictionary(attribute => attribute.MusicId);

        Musics = MusicAttributes.Select(pair => pair.Key)
                                .ToList();
        Musics.Sort();
        
        MusicsWithUra = MusicAttributes.Where(attribute => attribute.Value.HasUra)
                                       .Select(pair => pair.Key)
                                       .ToList();
        MusicsWithUra.Sort();
    }

    public static MusicAttributeManager Instance { get; } = new();

    public readonly List<uint> Musics;

    public readonly List<uint> MusicsWithUra;

    public bool MusicHasUra(uint musicId)
    {
        return MusicAttributes[musicId].HasUra;
    }
}