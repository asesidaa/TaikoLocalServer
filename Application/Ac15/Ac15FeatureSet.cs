namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15FeatureSet(
    bool NormalPlay,
    bool UserData,
    bool SelfBest,
    bool Crowns,
    bool InitialData,
    bool Folders,
    bool Telops,
    bool Recommendations,
    bool Taikojuku,
    bool Dani,
    bool ItemShop);
