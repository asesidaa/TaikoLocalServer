using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/{era}/[controller]")]
[Authorize]
public sealed partial class Ac15ProfileSettingsController(
    ITaikoDbContext context,
    IOptions<AuthSettings> authOptions) : BaseAdminController<Ac15ProfileSettingsController>
{
    private readonly ITaikoDbContext context = context;
    private readonly AuthSettings authSettings = authOptions.Value;

    [HttpGet("{baid}")]
    public async Task<ActionResult<Ac15ProfileSettingsDto>> Get(string era, uint baid)
    {
        if (!TryGetAc15Era(era, out var gameEra, out var badEra))
        {
            return badEra!;
        }

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        {
            return forbid;
        }

        return gameEra switch
        {
            GameEra.Blue => await GetBlue(baid),
            GameEra.Green => await GetGreen(baid),
            GameEra.Yellow => await GetYellow(baid),
            GameEra.Red => await GetRed(baid),
            _ => BadAc15Era(era)
        };
    }

    [HttpPut("{baid}")]
    public async Task<IActionResult> Put(string era, uint baid, Ac15ProfileSettingsUpdateDto request)
    {
        if (!TryGetAc15Era(era, out var gameEra, out var badEra))
        {
            return badEra!;
        }

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
        {
            return forbid;
        }

        return gameEra switch
        {
            GameEra.Blue => await PutBlue(baid, request),
            GameEra.Green => await PutGreen(baid, request),
            GameEra.Yellow => await PutYellow(baid, request),
            GameEra.Red => await PutRed(baid, request),
            _ => BadAc15Era(era)
        };
    }

    private async Task<ActionResult<Ac15ProfileSettingsDto>> GetForAc15Async<TSave, TDanScore>(
        uint baid,
        Func<uint, CancellationToken, ValueTask<TSave>> getSaveData,
        DbSet<TDanScore> danScores,
        Ac15EraProfile profile)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await getSaveData(baid, HttpContext.RequestAborted);
        var result = await Ac15ProfileSettingsService.GetAsync(
            user,
            saveData,
            danScores,
            profile,
            HttpContext.RequestAborted);

        return Ok(result.Setting);
    }

    private async Task<IActionResult> PutForAc15Async<TSave, TDanScore>(
        uint baid,
        Ac15ProfileSettingsUpdateDto request,
        Func<uint, CancellationToken, ValueTask<TSave>> getSaveData,
        DbSet<TDanScore> danScores,
        Ac15EraProfile profile)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await getSaveData(baid, HttpContext.RequestAborted);
        var result = await Ac15ProfileSettingsService.SaveAsync(
            user,
            saveData,
            danScores,
            profile,
            request,
            new Ac15ProfileEditPolicy(authSettings.AllowFreeProfileEditing),
            HttpContext.RequestAborted);
        if (result.Status == Ac15ProfileSettingsResultStatus.BadRequest)
        {
            return BadRequest(result.ErrorMessage);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static bool TryGetAc15Era(
        string era,
        out GameEra gameEra,
        [NotNullWhen(false)] out BadRequestObjectResult? badRequest)
    {
        if (!EraRoute.TryParse(era, out gameEra) || !Ac15EraProfiles.TryGet(gameEra, out _))
        {
            badRequest = BadAc15Era(era);
            return false;
        }

        badRequest = null;
        return true;
    }

    private static BadRequestObjectResult BadAc15Era(string era)
        => new($"Unsupported AC15 profile settings era '{era}'.");
}
