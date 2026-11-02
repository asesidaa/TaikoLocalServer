using TaikoLocalServer.Adapters.AdminApi.Authorization;
using TaikoLocalServer.Infrastructure.Persistence;

namespace TaikoLocalServer.Cli;

/// <summary>
/// Operator commands run against the database instead of starting the server:
/// <c>TaikoLocalServer admin promote|demote|set-password|reset-2fa &lt;accessCode&gt;</c>.
/// Admin rights otherwise only exist by editing the database by hand.
/// </summary>
public static class AdminCli
{
    private const string Usage = """
        Usage:
          TaikoLocalServer admin promote <accessCode>       Make the card's user an admin
          TaikoLocalServer admin demote <accessCode>        Remove admin rights
          TaikoLocalServer admin set-password <accessCode>  Set the card's web UI password (prompts)
          TaikoLocalServer admin reset-2fa <accessCode>     Turn off the card's 2-step verification (lost phone and codes)
        """;

    public static bool IsAdminCommand(string[] args) => args.Length > 0 && args[0] == "admin";

    public static async Task<int> RunAsync(string[] args, TaikoDbContext db)
    {
        if (args.Length != 3)
        {
            Console.WriteLine(Usage);
            return 1;
        }

        var (command, accessCode) = (args[1], args[2].Replace(" ", "").Replace("-", "").Replace(":", ""));
        var card = await db.Cards.FindAsync(accessCode);
        if (card is null)
        {
            Console.WriteLine($"Access code {accessCode} not found. Play one credit with this card first.");
            return 1;
        }

        switch (command)
        {
            case "promote":
            case "demote":
                var user = await db.UserData.FirstOrDefaultAsync(u => u.Baid == card.Baid);
                if (user is null)
                {
                    Console.WriteLine($"No user data for baid {card.Baid}.");
                    return 1;
                }
                user.IsAdmin = command == "promote";
                db.Activity.Add(new TaikoLocalServer.Domain.Entities.ActivityEntry
                    { Time = DateTime.UtcNow, Action = $"user.{command}", TargetBaid = card.Baid, Detail = "from the console" });
                await db.SaveChangesAsync();
                Console.WriteLine($"Baid {card.Baid} ({user.MyDonName}) is {(user.IsAdmin ? "now an admin" : "no longer an admin")}.");
                return 0;

            case "set-password":
                var credential = await db.Credentials.FindAsync(card.Baid);
                if (credential is null)
                {
                    Console.WriteLine($"No credential row for baid {card.Baid}.");
                    return 1;
                }
                var password = ReadPassword("New password: ");
                if (password.Length == 0 || password != ReadPassword("Confirm password: "))
                {
                    Console.WriteLine("Passwords were empty or did not match. Nothing changed.");
                    return 1;
                }
                credential.Salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                credential.Password = BCrypt.Net.BCrypt.HashPassword(password, credential.Salt);
                await db.SaveChangesAsync();
                Console.WriteLine($"Password set for baid {card.Baid}. Log in to the web UI with access code {accessCode}.");
                return 0;

            // The way back for a sole admin who lost their phone and recovery codes (no other admin can reset it in the
            // web UI). Same as the admin page's Reset: the secret, the recovery codes and the trusted browsers go.
            case "reset-2fa":
                var account = await db.Credentials.FindAsync(card.Baid);
                if (account?.TotpSecret is null)
                {
                    Console.WriteLine($"Baid {card.Baid} doesn't have 2-step verification on. Nothing changed.");
                    return 1;
                }
                TwoFactorCodes.Clear(account);
                db.TrustedBrowsers.RemoveRange(await db.TrustedBrowsers.Where(t => t.Baid == card.Baid).ToListAsync());
                db.Activity.Add(new TaikoLocalServer.Domain.Entities.ActivityEntry
                    { Time = DateTime.UtcNow, Action = "2fa.reset", TargetBaid = card.Baid, Detail = "from the console" });
                await db.SaveChangesAsync();
                Console.WriteLine($"2-step verification is off for baid {card.Baid}. They sign in with their password and can turn it on again in Settings.");
                return 0;

            default:
                Console.WriteLine(Usage);
                return 1;
        }
    }

    private static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        // Piped input (scripts, CI): no echo to suppress.
        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? "";

        var chars = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace) { if (chars.Count > 0) chars.RemoveAt(chars.Count - 1); continue; }
            if (!char.IsControl(key.KeyChar)) chars.Add(key.KeyChar);
        }
        Console.WriteLine();
        return new string(chars.ToArray());
    }
}
