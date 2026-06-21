using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared;

public static class GameProtocolApplicationParts
{
    public static HashSet<GameEra> ReadEnabledEras(IConfiguration serverSettingsConfig)
        => serverSettingsConfig.GetSection("Eras")
            .GetChildren()
            .Where(s => s.GetValue<bool>("Enabled"))
            .Select(s => Enum.Parse<GameEra>(s.Key, ignoreCase: true))
            .ToHashSet();

    public static void RemoveDisabledGameProtocolApplicationParts(
        ApplicationPartManager apm,
        IReadOnlySet<GameEra> enabledEras)
    {
        if (!enabledEras.Contains(GameEra.Green))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Green");
        }
        if (!enabledEras.Contains(GameEra.Blue))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Blue");
        }
        if (!enabledEras.Contains(GameEra.Yellow))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Yellow");
        }
        if (!enabledEras.Contains(GameEra.Red))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Red");
        }
        if (!enabledEras.Contains(GameEra.White))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.White");
        }
        if (!enabledEras.Contains(GameEra.Murasaki))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.Murasaki");
        }
        if (!enabledEras.Contains(GameEra.Nijiiro))
        {
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.WwR08");
            RemoveApplicationPart(apm, "TaikoLocalServer.Adapters.GameProtocol.CnR00");
        }
    }

    private static void RemoveApplicationPart(ApplicationPartManager apm, string assemblyName)
    {
        var part = apm.ApplicationParts.FirstOrDefault(p =>
            p is AssemblyPart a && a.Assembly.GetName().Name == assemblyName);
        if (part is not null)
        {
            apm.ApplicationParts.Remove(part);
        }
    }
}
