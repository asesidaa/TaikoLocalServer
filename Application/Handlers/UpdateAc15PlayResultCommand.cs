using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UpdateAc15PlayResultCommand(
    uint Baid,
    GameEra Era,
    Ac15PlayResultEnvelope PlayResultData) : IRequest<uint>;
