namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ShopSeasonPolicy
{
    public static Ac15ShopSeasonSeed BlueNewSeasonSeed()
        => Ac15ShopSeasonSeed.Zero;

    public static Ac15ShopSeasonSeed FirstSeasonFromSaveSeed(
        bool hasExistingSeasonState,
        uint saveTotalGetDonmedal,
        uint saveTotalUseDonmedal)
        => hasExistingSeasonState
            ? Ac15ShopSeasonSeed.Zero
            : new Ac15ShopSeasonSeed(saveTotalGetDonmedal, saveTotalUseDonmedal);
}

public readonly record struct Ac15ShopSeasonSeed(uint TotalGetDonmedal, uint TotalUseDonmedal)
{
    public static Ac15ShopSeasonSeed Zero { get; } = new(0, 0);
}
