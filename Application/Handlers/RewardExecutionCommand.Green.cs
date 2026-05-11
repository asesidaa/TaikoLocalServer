namespace TaikoLocalServer.Application.Handlers;

public partial class RewardExecutionCommandHandler
{
    public partial async ValueTask<CommonRewardExecutionResponse> Handle(
        RewardExecutionCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Green rewards for baid {Baid}", request.Baid);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);

        saveData.ToneFlg = SetBits(saveData.ToneFlg, request.GetToneNoes, GreenProtocolBytes.ToneFlagBytes);
        saveData.CostumeFlg1 = SetBits(saveData.CostumeFlg1, request.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg2 = SetBits(saveData.CostumeFlg2, request.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg3 = SetBits(saveData.CostumeFlg3, request.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg4 = SetBits(saveData.CostumeFlg4, request.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.CostumeFlg5 = SetBits(saveData.CostumeFlg5, request.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes);
        saveData.TitleFlg = SetBits(saveData.TitleFlg, request.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes);

        await context.SaveChangesAsync(cancellationToken);
        return new CommonRewardExecutionResponse { Result = 1 };
    }

    private static byte[] SetBits(byte[] source, IEnumerable<uint> ids, int byteCount)
    {
        var result = GreenProtocolBytes.FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }
}
