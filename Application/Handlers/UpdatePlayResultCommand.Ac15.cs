using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private static bool HasInvalidAc15MedalTotals(
        uint currentDonmedal,
        uint currentKatsumedal,
        CommonPlayResultData playResultData)
        => !CanAddAc15(currentDonmedal, playResultData.GetDonmedal)
           || !CanAddAc15(currentKatsumedal, playResultData.GetKatsumedal);

    private static void AddAc15Donmedals<TSeasonState>(
        TSeasonState? shopSeasonState,
        Action<uint> addToSave,
        uint getDonmedal)
        where TSeasonState : class, IAc15ShopSeasonState
    {
        if (shopSeasonState is null)
        {
            addToSave(getDonmedal);
            return;
        }

        shopSeasonState.TotalGetDonmedal += getDonmedal;
        shopSeasonState.UpdatedAt = DateTime.UtcNow;
    }

    private static bool CanAddAc15(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static DateTime ParseAc15PlayDatetimeOrNow(string playDatetime)
        => Ac15PlayDatetime.ParseOrNow(playDatetime);
}
