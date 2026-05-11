using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

public static class PathHelper
{
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

    public static string GetDataPath(GameEra era)
    {
        return Path.Combine(GetRootPath(), "data", era.ToString().ToLowerInvariant());
    }

    public static string GetSharedDataPath()
    {
        return Path.Combine(GetRootPath(), "data", "shared");
    }

    public static string GetDataTablePath(GameEra era)
    {
        return Path.Combine(GetDataPath(era), "datatable");
    }

    public static string GetDatatablePath()
    {
        return Path.Combine(GetDataPath(), "datatable");
    }
}
