using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

/// <summary>
/// Server-mode rules an admin changes at runtime (Admin › Access). One row (Id 1), created at first start with the
/// defaults or the values of a legacy AuthSettings.json. In Local mode they don't apply: there are no accounts.
/// </summary>
public class AccessRules
{
    public int Id { get; set; } = 1;

    /// <summary>Only admins may register and bind access codes.</summary>
    public bool OnlyAdmin { get; set; }

    /// <summary>Who can create a Web UI account for a card.</summary>
    public SignUpMode SignUpMode { get; set; }

    public bool AllowUserDelete { get; set; } = true;

    /// <summary>Users may pick any costume/title, not only unlocked ones.</summary>
    public bool AllowFreeProfileEditing { get; set; } = true;

    /// <summary>How many access codes a non-admin may bind to their account.</summary>
    public int AccessCodeLimit { get; set; } = 3;

    /// <summary>Failed Web UI sign-ins in a row that lock the account for <see cref="LockoutMinutes"/>.</summary>
    public int LockoutAttempts { get; set; } = 10;

    public int LockoutMinutes { get; set; } = 15;
}
