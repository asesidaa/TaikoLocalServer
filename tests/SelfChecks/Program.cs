// Self-checks for 2-step verification (Adapters.AdminApi/Authorization/TwoFactorCodes.cs) and the game files an admin
// may send from the browser (Infrastructure/GameDataCatalog/ExtractRunner.Match).
// A failed check prints its name and the process exits with 1.
using OtpNet;
using TaikoLocalServer.Adapters.AdminApi.Authorization;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

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

var inputs = new ExtractInputs(["Data/x64/datatable/musicinfo.bin", "Data/x64/model/body/"], ["Data/x64/movie/"],
    new ExtractInputs(["Data_mods/x64/model/body/"], ["Data_mods/x64/movie/"], null));
Check("a listed file is accepted with its contents", ExtractRunner.Match(inputs, "Data/x64/datatable/musicinfo.bin") == false);
Check("a file in a listed folder is accepted", ExtractRunner.Match(inputs, "Data/x64/model/body/a/b.numdlb") == false);
Check("a mods file is accepted", ExtractRunner.Match(inputs, "Data_mods/x64/model/body/a.nutexb") == false);
Check("a movie is accepted by name only", ExtractRunner.Match(inputs, "Data/x64/movie/mv_01.usm") == true);
Check("nothing under a movie subfolder", ExtractRunner.Match(inputs, "Data/x64/movie/x/y.usm") is null);
Check("an unlisted file is refused", ExtractRunner.Match(inputs, "Data/x64/datatable/other.bin") is null);
Check("a near miss on a listed file is refused", ExtractRunner.Match(inputs, "Data/x64/datatable/musicinfo.bin.exe") is null);
Check(".. can't climb out of a listed folder", ExtractRunner.Match(inputs, "Data/x64/model/body/../../../../evil.dll") is null);
Check("backslashes are refused", ExtractRunner.Match(inputs, "Data/x64/model/body/..\\..\\evil") is null);
Check("a drive or absolute path is refused", ExtractRunner.Match(inputs, "C:/Data/x64/model/body/a") is null && ExtractRunner.Match(inputs, "/Data/x64/model/body/a") is null);
Check("empty segments are refused", ExtractRunner.Match(inputs, "Data/x64/model/body//a") is null);

Console.WriteLine(failed == 0 ? "All checks passed." : $"{failed} check(s) failed.");
return failed == 0 ? 0 : 1;
