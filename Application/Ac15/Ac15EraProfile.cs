namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15EraProfile(
    GameEra Era,
    Ac15FeatureSet Features,
    Ac15ProtocolLimits Limits,
    Ac15WirePlacement WirePlacement,
    IAc15EraHooks Hooks);
