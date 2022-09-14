namespace TaikoLocalServer.Common.Utils;

public static class PathHelper
{
    public static string GetDataPath()
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
        return Path.Combine(parentPath.ToString(), "wwwroot", "data");
    }
}