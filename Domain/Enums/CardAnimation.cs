namespace TaikoLocalServer.Domain.Enums;

/// <summary>
/// The player card's don-chan animation preset. Stored by value (not the clip name, so clips can be renamed);
/// the WebUI maps each to a looping clip. Append new presets, never renumber.
/// </summary>
public enum CardAnimation
{
    Clear = 0,
    Celebrate = 1,
    Aggressive = 2,
    Miss = 3,
    FullCombo = 4,
    Pout = 5,
}
