namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>A Dani Dojo course set. <see cref="File"/> is its data file name for api/GameData/Files.</summary>
public record DanSetInfo(string Id, string Name, string File, bool IsDefault, int Courses);
