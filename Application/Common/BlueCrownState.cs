using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Common;

public enum BlueCrownState : ushort
{
    None = (ushort)Ac15CrownState.None,
    Clear = (ushort)Ac15CrownState.Clear,
    FullCombo = (ushort)Ac15CrownState.FullCombo
}
