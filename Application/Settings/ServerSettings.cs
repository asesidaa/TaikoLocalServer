namespace TaikoLocalServer.Application.Settings;

public class ServerSettings
{
    public bool EnableMoreSongs { get; set; }

    public int MoreSongsSize { get; set; } = TaikoLocalServer.Domain.DomainConstants.MusicIdMaxExpanded;
}
