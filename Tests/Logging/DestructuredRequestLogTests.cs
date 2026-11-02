using System.Globalization;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;
using BlueWire = TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

namespace TaikoLocalServer.Tests.Logging;

public sealed class DestructuredRequestLogTests
{
    [Fact]
    public void BluePlayResultRequestRendering_RemainsReadableForFullDumpLogs()
    {
        var request = new BlueWire.PlayResultRequest
        {
            Baid = 7654321,
            ChassisId = "CHASSIS-01",
            ShopId = "SHOP-99",
            PlayDatetime = "20260607123456",
            IsRight = true,
            CardType = 1,
            IsTwoPlayers = false,
            ReleaseSongNoes = [1001, 1002],
            GetDonmedal = 42,
            GetKatsumedal = 7,
            PlayerAge = 18,
            PlayMode = 3,
            AreaCode = 13,
            Reserved = [0x01, 0x02]
        };
        request.AryStageInfoes.Add(new BlueWire.PlayResultRequest.StageData
        {
            SongNo = 123,
            Level = 4,
            PlayResult = 3,
            PlayScore = 987650,
            GoodCnt = 700,
            OkCnt = 20,
            NgCnt = 1,
            PoundCnt = 12,
            ComboCnt = 720,
            OptionFlg = [0x10, 0x20],
            ToneFlg = [0x30],
            MusicCateg = 2,
            IsPushed = true,
            IsFavorite = false,
            IsRecent = true,
            IsPapamama = false,
            StageMode = 1,
            SelectedFolderId = 55
        });

        var rendered = RenderMessage("Blue PlayResult request: {@Request}", request);

        Assert.Contains("Blue PlayResult request:", rendered, StringComparison.Ordinal);
        Assert.Contains("Baid: 7654321", rendered, StringComparison.Ordinal);
        Assert.Contains("ChassisId: \"CHASSIS-01\"", rendered, StringComparison.Ordinal);
        Assert.Contains("ShopId: \"SHOP-99\"", rendered, StringComparison.Ordinal);
        Assert.Contains("PlayDatetime: \"20260607123456\"", rendered, StringComparison.Ordinal);
        Assert.Contains("PlayMode: 3", rendered, StringComparison.Ordinal);
        Assert.Contains("ReleaseSongNoes: [1001, 1002]", rendered, StringComparison.Ordinal);
        Assert.Contains("AryStageInfoes: [", rendered, StringComparison.Ordinal);
        Assert.Contains("SongNo: 123", rendered, StringComparison.Ordinal);
        Assert.Contains("PlayScore: 987650", rendered, StringComparison.Ordinal);
    }

    private static string RenderMessage(string messageTemplate, object value)
    {
        var sink = new CapturingSink();
        using var serilogLogger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();
        using var loggerFactory = new SerilogLoggerFactory(serilogLogger, dispose: false);
        var logger = loggerFactory.CreateLogger(nameof(DestructuredRequestLogTests));

        logger.LogInformation(messageTemplate, value);

        var formatter = new MessageTemplateTextFormatter("{Message}", CultureInfo.InvariantCulture);
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        formatter.Format(Assert.Single(sink.Events), writer);
        return writer.ToString();
    }

    private sealed class CapturingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new();

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
