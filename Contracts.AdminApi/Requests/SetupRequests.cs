namespace TaikoLocalServer.Contracts.AdminApi.Requests;

/// <summary>The setup wizard's first step: the code from the server's console.</summary>
public record SetupCodeRequest(string Code);

/// <summary>The games the cabinets run (ServerSettings.Eras.&lt;Game&gt;.Enabled).</summary>
public record SetupGamesRequest(string Code, IReadOnlyList<string> Games);

/// <summary>
/// Finish setup: how the server is used, the admin account (Server mode: the card's 20-digit access code and a
/// password), and who may sign up (Server mode).
/// </summary>
public record SetupFinishRequest(string Code, ServerMode Mode, string? AccessCode, string? Password, SignUpMode SignUp);
