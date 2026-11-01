namespace TaikoLocalServer.Contracts.AdminApi.Responses;

/// <summary>The operator's effective config files (Host/Configurations), read-only. Never carries secrets.</summary>
public record ServerConfigResponse(
    ServerMode Mode,
    IReadOnlyList<ServerEndpoint> Endpoints,
    string CertificatePath,
    string DbFileName,
    string MuchaUrl,
    string GameUrl,
    bool EnableMoreSongs,
    int MoreSongsSize,
    string GamePath);

public record ServerEndpoint(string Name, string Url);
