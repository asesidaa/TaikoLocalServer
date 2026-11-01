namespace TaikoLocalServer.Domain.Enums;

/// <summary>
/// The player card's don-chan animation preset. Stored by value (not the clip name, so clips can be renamed);
/// the WebUI maps each to a looping clip. Append new presets, never renumber.
/// </summary>
public enum CardAnimation
{
    Clear = 0,
    Celebrate = 1,
    Donderful = 2,
    Drumming = 3,
    Ready = 4,
    Pout = 5,
}
