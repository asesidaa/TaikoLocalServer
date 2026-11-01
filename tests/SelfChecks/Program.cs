// Self-checks for 2-step verification (Adapters.AdminApi/Authorization/TwoFactorCodes.cs).
// A failed check prints its name and the process exits with 1.
using OtpNet;
using TaikoLocalServer.Adapters.AdminApi.Authorization;
using TaikoLocalServer.Domain.Entities;

var failed = 0;
void Check(string name, bool ok)
{
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}");
    if (!ok) failed++;
}

var secret = TwoFactorCodes.NewSecret();
var credential = new Credential { Baid = 1, Password = "", Salt = "", TotpSecret = secret };
var code = new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp();

Check("a fresh code from the app passes", TwoFactorCodes.Verify(credential, code));
Check("the same code doesn't pass twice", !TwoFactorCodes.Verify(credential, code));
Check("a wrong code fails", !TwoFactorCodes.Verify(credential, code == "000000" ? "111111" : "000000"));
// One in a million: another key may produce the same 6 digits now.
Check("a code for another key fails", TwoFactorCodes.MatchTotp(TwoFactorCodes.NewSecret(), code) is null);
Check("a key that isn't base32 fails instead of throwing", TwoFactorCodes.MatchTotp("not base32!", code) is null);

var recovery = TwoFactorCodes.NewRecoveryCodes(credential);
Check("10 recovery codes", recovery.Count == 10 && TwoFactorCodes.RecoveryCodesLeft(credential) == 10);
Check("a recovery code passes, in lower case and without its dash", TwoFactorCodes.Verify(credential, recovery[0].ToLowerInvariant().Replace("-", "")));
Check("a recovery code works only once", !TwoFactorCodes.Verify(credential, recovery[0]));
Check("using one leaves 9", TwoFactorCodes.RecoveryCodesLeft(credential) == 9);
Check("new recovery codes replace the old ones", TwoFactorCodes.NewRecoveryCodes(credential) is var fresh && !TwoFactorCodes.Verify(credential, recovery[1]) && TwoFactorCodes.Verify(credential, fresh[0]));

TwoFactorCodes.Clear(credential);
Check("nothing passes once 2-step is off", !TwoFactorCodes.Verify(credential, new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp()));

Console.WriteLine(failed == 0 ? "All checks passed." : $"{failed} check(s) failed.");
return failed == 0 ? 0 : 1;
