using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TaikoWebUI.Services;

/// <summary>
/// Every API call goes through here, so a page never has to handle these itself:
/// <list type="bullet">
/// <item>No connection: <see cref="AppStatus"/> shows the bar and retries. A save waits for the connection and is
/// sent then; a read fails (the page's "Couldn't load").</item>
/// <item>401 on a signed-in request (the session expired, or "sign out everywhere"): the shell asks for the password
/// again and the same request is sent with the new token, so the page and its edits stay as they were. That's why an
/// input mistake (wrong password, wrong code) must answer 400, never 401.</item>
/// <item>503 with a server state: the banner or offline page updates; a save refused for maintenance shows a toast.</item>
/// </list>
/// </summary>
public sealed class AppStatusHandler(AppStatus status) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";
        // Signing in and out never waits: signing out offline still forgets the token, and a wrong password is not a sign-out.
        var account = path.StartsWith("/api/Auth/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/api/Sessions", StringComparison.OrdinalIgnoreCase);
        var save = request.Method != HttpMethod.Get && request.Method != HttpMethod.Head;
        var held = save && !account;
        // Buffered, so the request can be sent again.
        var body = request.Content is null ? null : await request.Content.ReadAsByteArrayAsync(cancellationToken);

        var attempt = request;
        while (true)
        {
            if (held && status.Offline)
            {
                status.SaveHeld();
                await status.WhenOnline(cancellationToken);
            }
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(attempt, cancellationToken);
            }
            catch (HttpRequestException) when (!cancellationToken.IsCancellationRequested)
            {
                status.Lost();
                if (!held) throw;
                attempt = Copy(request, body);
                continue;
            }
            status.Reached();

            if (response.StatusCode == HttpStatusCode.Unauthorized && attempt.Headers.Authorization is { } sent && !account)
            {
                // Sent with the old token before signing in again finished: just send it with the new one.
                var token = status.LatestToken is { } latest && latest != sent.Parameter ? latest : await status.SignInAgain();
                if (token is null) return response;
                attempt = Copy(request, body);
                attempt.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                continue;
            }
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable && await StateOf(response) is { } state)
                status.Refused(state, save);
            return response;
        }
    }

    private static HttpRequestMessage Copy(HttpRequestMessage request, byte[]? body)
    {
        var copy = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
        foreach (var header in request.Headers) copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
        if (body is not null)
        {
            copy.Content = new ByteArrayContent(body);
            foreach (var header in request.Content!.Headers) copy.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        return copy;
    }

    // ServerStateFilter's 503: { message, state }.
    private static async Task<ServerState?> StateOf(HttpResponseMessage response)
    {
        await response.Content.LoadIntoBufferAsync();
        try
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!json.RootElement.TryGetProperty("state", out var state)) return null;
            return state.ValueKind == JsonValueKind.Number ? (ServerState)state.GetInt32()
                : Enum.TryParse<ServerState>(state.GetString(), true, out var named) ? named : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
