using System.Text.Json.Nodes;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Handlers;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// The setup wizard (/setup), open until setup is finished. Every step proves the operator is at the server with the
/// setup code from its console. In between, the same code (the X-Setup-Code header) opens the admin pages the wizard
/// uses (extract, the game folder picker): there's no admin to sign in yet.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[OpenInEveryServerState]
[EnableRateLimiting(DependencyInjection.AuthRateLimitPolicy)]
public class SetupController(ITaikoDbContext context, IServerAccess access, IServerControl server, IOptions<ServerSettings> settings)
    : BaseAdminController<SetupController>
{
    /// <summary>Checks the setup code (step 1). It isn't used up until setup is finished.</summary>
    [HttpPost("Code")]
    public IActionResult Code(SetupCodeRequest request) => Check(request.Code) ?? NoContent();

    /// <summary>
    /// The games the cabinets run. Extract only offers games that were on when the server started, so a change
    /// restarts it.
    /// </summary>
    [HttpPost("Games")]
    public async Task<IActionResult> Games(SetupGamesRequest request)
    {
        if (Check(request.Code) is { } refused) return refused;
        var games = new HashSet<GameEra>();
        foreach (var name in request.Games)
        {
            if (!EraRoute.TryParse(name, out var game)) return EraRoute.BadEra(name);
            games.Add(game);
        }
        if (games.Count == 0) return BadRequest(new { message = "Pick at least one game." });

        var running = settings.Value.Eras.Where(e => e.Value.Enabled && Enum.TryParse<GameEra>(e.Key, out _))
            .Select(e => Enum.Parse<GameEra>(e.Key)).ToHashSet();
        if (running.SetEquals(games)) return Ok(new SetupResponse(false));

        await server.UpdateSettingsAsync(s =>
        {
            if (s["Eras"] is not JsonObject eras) s["Eras"] = eras = [];
            foreach (var game in Enum.GetValues<GameEra>())
            {
                if (eras[game.ToString()] is not JsonObject era)
                {
                    if (!games.Contains(game)) continue;
                    eras[game.ToString()] = era = [];
                }
                era["Enabled"] = games.Contains(game);
            }
        }, HttpContext.RequestAborted);
        server.Restart();
        return Ok(new SetupResponse(true));
    }

    /// <summary>
    /// The last step: the admin (Server mode), who may sign up, and how the server is used. Setup is then finished:
    /// the code stops working. A change of mode restarts the server.
    /// </summary>
    [HttpPost("Finish")]
    public async Task<IActionResult> Finish(SetupFinishRequest request)
    {
        if (Check(request.Code) is { } refused) return refused;
        if (!Enum.IsDefined(request.Mode)) return BadRequest(new { message = "Pick Local or Server." });

        if (request.Mode == ServerMode.Server)
        {
            var accessCode = request.AccessCode ?? "";
            if (accessCode.Length != 20 || !accessCode.All(char.IsAsciiDigit))
                return BadRequest(new { message = "Access codes are 20 digits." });
            if ((request.Password ?? "").Length < 8)
                return BadRequest(new { message = "Use at least 8 characters for the password." });
            if (!Enum.IsDefined(request.SignUp))
                return BadRequest(new { message = "Pick who can sign up." });

            // The admin's card doesn't have to have played here: its player is made as the game would on its first
            // scan. A card that already has an account becomes the admin, with this password.
            if (await context.Cards.FindAsync([accessCode], HttpContext.RequestAborted) is null)
                await Mediator.Send(new AddMyDonEntryCommand(GameEra.Nijiiro, accessCode, "Admin", 0), HttpContext.RequestAborted);
            var card = await context.Cards.FindAsync([accessCode], HttpContext.RequestAborted);
            var credential = card is null ? null : await context.Credentials.FindAsync([card.Baid], HttpContext.RequestAborted);
            var user = card is null ? null : await context.UserData.FirstOrDefaultAsync(u => u.Baid == card.Baid, HttpContext.RequestAborted);
            if (credential is null || user is null)
                return BadRequest(new { message = "Couldn't make the admin's account." });
            credential.Salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            credential.Password = BCrypt.Net.BCrypt.HashPassword(request.Password, credential.Salt);
            user.IsAdmin = true;
            await context.SaveChangesAsync(HttpContext.RequestAborted);

            var rules = access.Rules;
            rules.SignUpMode = request.SignUp;
            await access.UpdateRulesAsync(rules, HttpContext.RequestAborted);
        }

        var restart = request.Mode != access.Mode;
        if (restart || settings.Value.Mode != request.Mode)
            await server.UpdateSettingsAsync(s => s["Mode"] = request.Mode.ToString(), HttpContext.RequestAborted);
        await access.FinishSetupAsync(HttpContext.RequestAborted);
        Logger.LogInformation("Setup finished: {Mode} mode", request.Mode);
        if (restart) server.Restart();
        return Ok(new SetupResponse(restart));
    }

    // Setup is open and the code is right, or why not. A wrong code is an input mistake: 400, never 401.
    private IActionResult? Check(string? code) =>
        !access.SetupPending ? Conflict(new { message = "Setup is already finished." })
        : !access.IsSetupCode(code) ? BadRequest(new { message = "That setup code isn't right. It changes every time the server starts, so check the newest line in the console." })
        : null;
}
