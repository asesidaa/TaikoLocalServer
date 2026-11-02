using OtpNet;
using TaikoLocalServer.Adapters.AdminApi.Authorization;

namespace TaikoLocalServer.Tests.AdminApi;

// 2-step verification (Adapters.AdminApi/Authorization/TwoFactorCodes.cs).
public sealed class TwoFactorCodesTests
{
    private readonly string secret = TwoFactorCodes.NewSecret();
    private readonly Credential credential;

    public TwoFactorCodesTests() => credential = new Credential { Baid = 1, Password = "", Salt = "", TotpSecret = secret };

    private string Now() => new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp();

    [Fact]
    public void AppCode_PassesOnce()
    {
        var code = Now();
        Assert.True(TwoFactorCodes.Verify(credential, code));
        Assert.False(TwoFactorCodes.Verify(credential, code));
    }

    [Fact]
    public void WrongCode_Fails()
    {
        var code = Now();
        Assert.False(TwoFactorCodes.Verify(credential, code == "000000" ? "111111" : "000000"));
    }

    [Fact]
    public void OtherKeyOrBadKey_Fails()
    {
        // One in a million: another key may produce the same 6 digits now.
        Assert.Null(TwoFactorCodes.MatchTotp(TwoFactorCodes.NewSecret(), Now()));
        Assert.Null(TwoFactorCodes.MatchTotp("not base32!", Now()));
    }

    [Fact]
    public void RecoveryCodes_WorkOnceEachInAnyCase()
    {
        var recovery = TwoFactorCodes.NewRecoveryCodes(credential);
        Assert.Equal(10, recovery.Count);
        Assert.Equal(10, TwoFactorCodes.RecoveryCodesLeft(credential));
        Assert.True(TwoFactorCodes.Verify(credential, recovery[0].ToLowerInvariant().Replace("-", "")));
        Assert.False(TwoFactorCodes.Verify(credential, recovery[0]));
        Assert.Equal(9, TwoFactorCodes.RecoveryCodesLeft(credential));

        var fresh = TwoFactorCodes.NewRecoveryCodes(credential);
        Assert.False(TwoFactorCodes.Verify(credential, recovery[1]));
        Assert.True(TwoFactorCodes.Verify(credential, fresh[0]));
    }

    [Fact]
    public void Cleared_NothingPasses()
    {
        TwoFactorCodes.Clear(credential);
        Assert.False(TwoFactorCodes.Verify(credential, Now()));
    }
}
