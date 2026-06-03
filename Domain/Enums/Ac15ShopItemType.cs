namespace TaikoLocalServer.Domain.Enums;

public enum Ac15ShopItemType : uint
{
    Song = 1,
    Tone = 2,
    Kigurumi = 3,
    Body = 4,
    Head = 5,
    Face = 6,
    Puchi = 7
}

public static class Ac15ShopItemTypeExtensions
{
    public static uint ToProtocolValue(this Ac15ShopItemType itemType)
        => (uint)itemType;

    public static bool IsSupported(this Ac15ShopItemType itemType)
        => itemType is Ac15ShopItemType.Song
            or Ac15ShopItemType.Tone
            or Ac15ShopItemType.Kigurumi
            or Ac15ShopItemType.Body
            or Ac15ShopItemType.Head
            or Ac15ShopItemType.Face
            or Ac15ShopItemType.Puchi;
}
