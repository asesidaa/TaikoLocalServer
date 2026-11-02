namespace TaikoLocalServer.Application.Ac15;

public enum Ac15CrownWirePlacement
{
    Absent = 0,
    DedicatedEndpoint = 1,
    UserData = 2
}

public sealed record Ac15WirePlacement(
    Ac15CrownWirePlacement CrownPlacement,
    bool HasInitialDataItemShopRows,
    bool HasInitialDataLegalTermsRows,
    bool HasTokkunTutorialFlagInUserData);
