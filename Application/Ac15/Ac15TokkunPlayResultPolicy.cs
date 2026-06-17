using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15TokkunPlayResultPolicy
{
    public static bool IsTokkun(Ac15PlayResultEnvelope playResultData)
        => playResultData.Metadata.PlayMode == (uint)PlayMode.Tokkun;
}
