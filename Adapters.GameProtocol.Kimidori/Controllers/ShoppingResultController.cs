namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Controllers;

[ApiController]
public sealed class ShoppingResultController(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService)
    : BaseProtocolController<ShoppingResultController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/shoppingresult.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> ShoppingResult([FromBody] ShoppingResultRequest request)
    {
        Logger.LogInformation("Kimidori ShoppingResult request: {@Request}", request);

        var saveData = request.Baid == 0
            ? null
            : await context.UserSaveDataKimidori.FindAsync([request.Baid], HttpContext.RequestAborted);
        var kimidori = gameDataService.Kimidori();
        if (saveData is null)
        {
            if (request.Baid != 0)
            {
                Logger.LogWarning("Kimidori ShoppingResult for missing baid {Baid}", request.Baid);
            }

            return Ok(new ShoppingResultResponse
            {
                Result = 1,
                SongHashVer = kimidori.SongHashVersion,
                HashReleaseSongFlg = Ac15SongHashCodec.CompactBitset(
                    new byte[Ac15EraProfiles.Kimidori.Limits.SongFlagBytes],
                    kimidori.SongHashTable),
                ToneFlg = new byte[Ac15EraProfiles.Kimidori.Limits.ToneFlagBytes],
                CostumeFlg1 = new byte[Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes],
                CostumeFlg2 = new byte[Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes],
                CostumeFlg3 = new byte[Ac15EraProfiles.Kimidori.Limits.CostumeFlagBytes]
            });
        }

        var limits = Ac15EraProfiles.Kimidori.Limits;
        if (request.UseDonpoint <= uint.MaxValue - saveData.TotalUseDonpoint)
        {
            saveData.TotalUseDonpoint += request.UseDonpoint;
        }
        else
        {
            Logger.LogWarning(
                "Ignoring Kimidori ShoppingResult Don point spend overflow for baid {Baid}: current={Current}, incoming={Incoming}",
                request.Baid,
                saveData.TotalUseDonpoint,
                request.UseDonpoint);
        }

        saveData.ToneFlg = Ac15ProtocolBytes.OrBitsets(saveData.ToneFlg, request.ToneFlg, limits.ToneFlagBytes);
        saveData.CostumeFlg1 = Ac15ProtocolBytes.OrBitsets(saveData.CostumeFlg1, request.CostumeFlg1, limits.CostumeFlagBytes);
        saveData.CostumeFlg2 = Ac15ProtocolBytes.OrBitsets(saveData.CostumeFlg2, request.CostumeFlg2, limits.CostumeFlagBytes);
        saveData.CostumeFlg3 = Ac15ProtocolBytes.OrBitsets(saveData.CostumeFlg3, request.CostumeFlg3, limits.CostumeFlagBytes);
        saveData.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(
            saveData.ReleaseSongFlg,
            request.AryShoppingSongNoes ?? [],
            limits.SongFlagBytes);
        await context.SaveChangesAsync(HttpContext.RequestAborted);

        return Ok(new ShoppingResultResponse
        {
            Result = 1,
            TotalGetDonpoint = saveData.TotalGetDonpoint,
            TotalUseDonpoint = saveData.TotalUseDonpoint,
            ToneFlg = Ac15ProtocolBytes.FixedOrZero(saveData.ToneFlg, limits.ToneFlagBytes),
            CostumeFlg1 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg1, limits.CostumeFlagBytes),
            CostumeFlg2 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg2, limits.CostumeFlagBytes),
            CostumeFlg3 = Ac15ProtocolBytes.FixedOrZero(saveData.CostumeFlg3, limits.CostumeFlagBytes),
            SongHashVer = kimidori.SongHashVersion,
            HashReleaseSongFlg = Ac15SongHashCodec.CompactBitset(
                Ac15ProtocolBytes.FixedOrZero(saveData.ReleaseSongFlg, limits.SongFlagBytes),
                kimidori.SongHashTable)
        });
    }
}
