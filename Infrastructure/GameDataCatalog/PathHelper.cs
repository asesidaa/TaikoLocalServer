using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

public static class PathHelper
{
    public static string GetRootPath()
    {
        return ResolveRootPath(Environment.ProcessPath, AppContext.BaseDirectory);
    }

    public static string ResolveRootPath(string? processPath, string baseDirectory)
    {
        var applicationDirectory = ResolveApplicationDirectory(processPath, baseDirectory);
        return Path.Combine(applicationDirectory, "wwwroot");
    }

    private static string ResolveApplicationDirectory(string? processPath, string baseDirectory)
    {
        if (!string.IsNullOrWhiteSpace(processPath)
            && !string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            var parentPath = Directory.GetParent(processPath);
            if (parentPath is not null)
            {
                return parentPath.FullName;
            }
        }

        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new ApplicationException();
        }

        return Path.GetFullPath(baseDirectory);
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
