using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using TaikoLocalServer.Adapters.GameProtocol.Shared;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroApplicationPartTests
{
    private const string MomoiroAssemblyName = "TaikoLocalServer.Adapters.GameProtocol.Momoiro";

    [Fact]
    public void RemoveDisabledGameProtocolApplicationParts_MomoiroDisabled_RemovesMomoiroAssemblyPart()
    {
        var manager = BuildManagerWithMomoiroPart();

        GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(
            manager,
            new HashSet<GameEra>());

        Assert.DoesNotContain(manager.ApplicationParts, IsMomoiroPart);
    }

    [Fact]
    public void RemoveDisabledGameProtocolApplicationParts_MomoiroEnabled_RetainsMomoiroAssemblyPart()
    {
        var manager = BuildManagerWithMomoiroPart();
        var enabledEras = new HashSet<GameEra>
        {
            Enum.Parse<GameEra>("Momoiro", ignoreCase: true)
        };

        GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(
            manager,
            enabledEras);

        Assert.Contains(manager.ApplicationParts, IsMomoiroPart);
    }

    private static ApplicationPartManager BuildManagerWithMomoiroPart()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(MomoiroAssemblyName),
            AssemblyBuilderAccess.Run);

        var manager = new ApplicationPartManager();
        manager.ApplicationParts.Add(new AssemblyPart(assembly));
        return manager;
    }

    private static bool IsMomoiroPart(ApplicationPart part)
        => part is AssemblyPart assemblyPart
           && assemblyPart.Assembly.GetName().Name == MomoiroAssemblyName;
}
