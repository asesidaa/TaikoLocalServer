using System.Text.Json;

namespace TaikoLocalServer.Common.Utils;

public class MusicAttributeManager
{
    public readonly Dictionary<uint,MusicAttribute> MusicAttributes;

    static MusicAttributeManager()
    {
    }

    private MusicAttributeManager()
    {
        var dataPath = PathHelper.GetDataPath();
        var filePath = Path.Combine(dataPath, Constants.MUSIC_ATTRIBUTE_FILE_NAME);
        var jsonString = File.ReadAllText(filePath);
        
        var result = JsonSerializer.Deserialize<List<MusicAttribute>>(jsonString);
        if (result is null)
        {
            throw new ApplicationException("Cannot parse music attribute json!");
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
}