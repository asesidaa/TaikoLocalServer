namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

public static class PathHelper
{
    /// <summary>What tools/extract writes besides the datatables (models, textures, UI art, fonts, tones), under wwwroot.</summary>
    public const string GameDataFolder = "game_data";

    /// <summary>The folder used to be called don-model: rename an existing one so it needn't be extracted again.</summary>
    public static void MoveLegacyGameDataFolder()
    {
        var old = Path.Combine(GetRootPath(), "don-model");
        var current = Path.Combine(GetRootPath(), GameDataFolder);
        if (Directory.Exists(old) && !Directory.Exists(current))
            Directory.Move(old, current);
    }

    public static string GetRootPath()
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
        return Path.Combine(parentPath.ToString(), "wwwroot");
    }
    
    public static string GetDataPath()
    {
        return Path.Combine(GetRootPath(), "data");
    }

	public static string GetDatatablePath()
	{
		return Path.Combine(GetDataPath(), "datatable");
	}
}