using SharedProject.Models;

namespace TaikoLocalServer.Models.Application;

public class CommonGetFolderResponse
{
    public uint Result { get; set; }

    public List<EventFolderData> AryEventfolderDatas { get; set; } = [];
}