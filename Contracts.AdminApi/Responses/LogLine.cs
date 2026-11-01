namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>One server log event for Server Management › Logs (access codes already masked). Level: INF, WRN, ERR, …</summary>
public record LogLine(long Id, DateTime Time, string Level, string Message, string? Exception);
