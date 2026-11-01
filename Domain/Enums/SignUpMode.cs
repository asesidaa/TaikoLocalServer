namespace TaikoLocalServer.Domain.Enums;

/// <summary>Who can create a Web UI account for a card (Server mode, Admin › Access).</summary>
public enum SignUpMode
{
    /// <summary>Anyone with a card's access code.</summary>
    Open = 0,
    /// <summary>They prove the card with its last play time, or an admin's invite code.</summary>
    LastPlayTime = 1,
    /// <summary>Only with an admin's invite code.</summary>
    InviteOnly = 2,
    /// <summary>No sign-ups at all (the first admin still uses the setup code).</summary>
    Closed = 3,
}
