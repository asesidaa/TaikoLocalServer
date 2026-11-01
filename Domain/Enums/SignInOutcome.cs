namespace TaikoLocalServer.Domain.Enums;

/// <summary>What happened to a Web UI sign-in (Access › Sign-ins). Unlocked marks an admin clearing a lockout.</summary>
public enum SignInOutcome
{
    SignedIn = 0,
    WrongPassword = 1,
    UnknownAccessCode = 2,
    NotRegistered = 3,
    AccountLocked = 4,
    CardBanned = 5,
    Unlocked = 6,
    WrongTwoFactorCode = 7,
}
