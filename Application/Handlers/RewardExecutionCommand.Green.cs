namespace TaikoLocalServer.Application.Handlers;

public partial class RewardExecutionCommandHandler
{
    public partial async ValueTask<CommonRewardExecutionResponse> Handle(
        RewardExecutionCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying Green rewards for baid {Baid}", request.Baid);
        var saveData = await context.GetOrCreateGreenSaveDataAsync(request.Baid, cancellationToken);

        if (!AllAlreadyUnlocked(saveData.ToneFlg, request.GetToneNoes, GreenProtocolBytes.ToneFlagBytes)
            || !AllAlreadyUnlocked(saveData.CostumeFlg1, request.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes)
            || !AllAlreadyUnlocked(saveData.CostumeFlg2, request.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes)
            || !AllAlreadyUnlocked(saveData.CostumeFlg3, request.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes)
            || !AllAlreadyUnlocked(saveData.CostumeFlg4, request.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes)
            || !AllAlreadyUnlocked(saveData.CostumeFlg5, request.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes)
            || !AllAlreadyUnlocked(saveData.TitleFlg, request.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes))
        {
            logger.LogWarning("Rejecting unknown Green reward ids for baid {Baid}", request.Baid);
            return new CommonRewardExecutionResponse { Result = 0 };
        }

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

    private static bool HasBit(byte[] source, uint id, int byteCount)
    {
        if (id >= byteCount * 8)
        {
            return false;
        }

        var fixedBytes = GreenProtocolBytes.FixedOrZero(source, byteCount);
        return (fixedBytes[id >> 3] & (1 << ((int)id & 7))) != 0;
    }

    private static bool AllAlreadyUnlocked(byte[] source, IEnumerable<uint> ids, int byteCount)
        => ids.All(id => HasBit(source, id, byteCount));

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
