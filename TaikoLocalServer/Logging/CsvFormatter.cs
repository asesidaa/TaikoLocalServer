using Serilog.Events;
using Serilog.Formatting;

namespace TaikoLocalServer.Logging;

public class CsvFormatter: ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        logEvent.Properties.TryGetValue("ChassisId", out var chassisId);
        logEvent.Properties.TryGetValue("ShopId", out var shopId);
        logEvent.Properties.TryGetValue("Baid", out var baid);
        logEvent.Properties.TryGetValue("PlayedAt", out var playedAt);
        logEvent.Properties.TryGetValue("IsRight", out var isRight);
        logEvent.Properties.TryGetValue("Type", out var type);
        logEvent.Properties.TryGetValue("Amount", out var amount);
        
        output.Write(logEvent.Timestamp.ToString("yyyy-MM-dd"));
        output.Write(",");
        output.Write(chassisId);
        output.Write(",");
        output.Write(shopId);
        output.Write(",");
        output.Write(baid);
        output.Write(",");
        output.Write(playedAt);
        output.Write(",");
        output.Write(isRight);
        output.Write(",");
        output.Write(type);
        output.Write(",");
        output.Write(amount);
        output.WriteLine();
    }
}