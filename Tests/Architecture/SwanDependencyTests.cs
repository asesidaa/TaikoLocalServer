namespace TaikoLocalServer.Tests.Architecture;

public sealed class SwanDependencyTests
{
    [Fact]
    public void RuntimeProjects_DoNotReferenceSwanPackages()
    {
        var repoRoot = FindRepoRoot();
        var forbidden = new[] { "Swan.Core", "Swan.Logging" };
        var files = Directory.EnumerateFiles(repoRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(IsRuntimeProject)
            .Append(Path.Combine(repoRoot, "Directory.Packages.props"));

        var packageReferences = files
            .SelectMany(path => forbidden
                .Where(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal))
                .Select(token => $"{Path.GetRelativePath(repoRoot, path)} contains {token}"))
            .ToArray();

        Assert.Empty(packageReferences);
    }

    [Fact]
    public void RuntimeSource_DoesNotUseSwanHelpers()
    {
        var repoRoot = FindRepoRoot();
        var forbidden = new[]
        {
            "Swan.Formatters",
            "Swan.Mapping",
            ".Stringify()",
            "CopyPropertiesToNew",
            "CopyOnlyPropertiesTo"
        };

        var violations = Directory.EnumerateFiles(repoRoot, "*.cs", SearchOption.AllDirectories)
            .Where(IsRuntimeSource)
            .SelectMany(path => forbidden
                .Where(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal))
                .Select(token => $"{Path.GetRelativePath(repoRoot, path)} contains {token}"))
            .ToArray();

        Assert.Empty(violations);
    }

    private static bool IsRuntimeProject(string path)
    {
        var relative = Path.GetRelativePath(FindRepoRoot(), path);
        return !relative.StartsWith("Tests", StringComparison.Ordinal)
               && !relative.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               && !relative.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static bool IsRuntimeSource(string path)
    {
        var relative = Path.GetRelativePath(FindRepoRoot(), path);
        return !relative.StartsWith("Tests", StringComparison.Ordinal)
               && !relative.StartsWith("docs", StringComparison.Ordinal)
               && !relative.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               && !relative.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               && !relative.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               && !relative.Contains($"{Path.DirectorySeparatorChar}wwwroot{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
