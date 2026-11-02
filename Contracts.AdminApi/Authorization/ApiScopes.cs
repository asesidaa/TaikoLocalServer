namespace TaikoLocalServer.Contracts.AdminApi.Authorization;

/// <summary>What an API token may do (Access › API tokens). A token acts as the admin who created it, limited to these.</summary>
public static class ApiScopes
{
    public const string ReadProfiles = "read:profiles";
    public const string ReadScores = "read:scores";
    public const string ReadAssets = "read:assets";
    public const string AdminUsers = "admin:users";
    public const string AdminServer = "admin:server";

    /// <summary>The claim a token's principal carries once per scope.</summary>
    public const string Claim = "scope";

    /// <summary>The claim that marks a token's principal (its id), so the scope filter knows to check.</summary>
    public const string TokenClaim = "api_token";

    /// <summary>Every token secret starts with this; it's how a request is told apart from a Web UI sign-in.</summary>
    public const string Prefix = "tls_";

    public static readonly (string Scope, string Description)[] All =
    [
        (ReadProfiles, "Profiles: settings, cards, titles"),
        (ReadScores, "Scores: best scores, play history, Dani Dojo results"),
        (ReadAssets, "Game art: don-chan models, nameplates, scoreboard"),
        (AdminUsers, "Users: invite codes, password resets, access codes, bans"),
        (AdminServer, "Server: game data, extract, Dani Dojo sets, rules, logs"),
    ];
}
