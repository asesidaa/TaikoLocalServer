using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Server Management › Logs: the recent backlog and a live stream (server-sent events). Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer)]
public class LogsController(ILogBuffer logs) : BaseAdminController<LogsController>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [HttpGet]
    public IReadOnlyList<LogLine> Get() => logs.Snapshot();

    /// <summary>text/event-stream: one "data: {line}" per new event, until the client disconnects.</summary>
    [HttpGet("Stream")]
    public async Task Stream()
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        var ct = HttpContext.RequestAborted;
        await Response.WriteAsync(": connected\n\n", ct);
        await Response.Body.FlushAsync(ct);
        try
        {
            await foreach (var line in logs.Subscribe(ct))
            {
                await Response.WriteAsync($"data: {JsonSerializer.Serialize(line, Json)}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // The page closed the stream.
        }
    }
}
