namespace TaikoLocalServer.Domain.Enums;

/// <summary>How many pixels the player cards' 3D don-chan is drawn with (the embed's <c>quality</c>).</summary>
public enum RenderQuality
{
    /// <summary>1x: the lightest, for slow phones and PCs.</summary>
    Low = 0,

    /// <summary>The screen's own pixel density.</summary>
    Standard = 1,

    /// <summary>At least 2x (supersampled on ordinary screens): the smoothest edges.</summary>
    High = 2,
}
