namespace TaikoLocalServer.Domain.Enums;

/// <summary>2-step verification (an authenticator app) for Web UI sign-in, set in Admin › Access.</summary>
public enum TwoFactorMode
{
    Off = 0,
    /// <summary>Anyone may turn it on in Settings › Security.</summary>
    Optional = 1,
    /// <summary>Optional for players; admins get admin access only once it's on.</summary>
    RequiredForAdmins = 2,
}
