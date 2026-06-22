using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using TaikoLocalServer.Application;
using TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers;
using TaikoLocalServer.Application.ServerData;
using FinalControllers = TaikoLocalServer.Adapters.GameProtocol.Murasaki.Controllers;
using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Murasaki.Wire;
using LegacyWire = TaikoLocalServer.Adapters.GameProtocol.Murasaki.LegacyWire;

namespace TaikoLocalServer.Tests.Murasaki;

public sealed class MurasakiProtocolVersionCompatibilityTests
{
    [Fact]
    public void GetFolder_FinalSchemaSerializesRepeatedEventFolderRows()
    {
        var response = FolderDataMappers.Map(new CommonGetFolderResponse
        {
            Result = 1,
            AryEventfolderDatas =
            [
                new EventFolderData { FolderId = 7, SongNoes = [101, 102] },
                new EventFolderData { FolderId = 9, SongNoes = [201] }
            ]
        });

        var rows = response.AryEventfolderDatas.ToArray();
        Assert.Equal(1u, response.Result);
        Assert.Equal([7u, 9u], rows.Select(row => row.FolderId).ToArray());
        Assert.Equal([101u, 102u], rows[0].SongNoes);
        Assert.Equal([201u], rows[1].SongNoes);

        var fields = ReadTopLevelFieldNumbers(Serialize(response));
        Assert.Contains(2, fields);
        Assert.DoesNotContain(3, fields);
    }

    [Fact]
    public void GetFolder_CompatibilitySchemaPreservesScalarFolderShape()
    {
        var response = LegacyWire.LegacyFolderDataMappers.MapSingle(
            new CommonGetFolderResponse
            {
                Result = 1,
                AryEventfolderDatas =
                [
                    new EventFolderData { FolderId = 7, SongNoes = [101, 102] },
                    new EventFolderData { FolderId = 9, SongNoes = [201] }
                ]
            },
            requestedFolderId: 7);

        Assert.Equal(1u, response.Result);
        Assert.Equal(7u, response.FolderId);
        Assert.Equal([101u, 102u], response.SongNoes);

        var fields = ReadTopLevelFieldNumbers(Serialize(response));
        Assert.Contains(2, fields);
        Assert.Contains(3, fields);
    }

    [Fact]
    public async Task Taikojuku_FinalRouteReturnsCatalogPacks()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync(CreateDanCatalog(1));

        var response = await InvokeMaybeAsync(
            new FinalControllers.TaikojukuController(),
            controller => controller.FinalTaikojuku(new FinalWire.TaikojukuRequest
            {
                ChassisId = "268410000000",
                ShopId = "JPN0JPN0123",
                GetDans = [1]
            }),
            value => Assert.IsType<FinalWire.TaikojukuResponse>(value),
            CreateServices(fixture));

        Assert.Equal(1u, response.Result);
        Assert.Contains(response.AryJukupackDatas, pack => pack.GetDan == 1);
    }

    [Fact]
    public async Task PlayResult_FinalDanModeCreatesMurasakiDanRows()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync(CreateDanCatalog(1));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataMurasaki.Add(UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var response = await InvokeMaybeAsync(
            new FinalControllers.PlayResultController(),
            controller => controller.FinalPlayResult(CreateDanWireRequest(1)),
            value => Assert.IsType<FinalWire.PlayResultResponse>(value),
            CreateServices(fixture));

        Assert.Equal(1u, response.Result);
        var dan = Assert.Single(await fixture.Context.DanScoreDataMurasaki
            .Include(row => row.DanStageScoreData)
            .Where(row => row.Baid == 1)
            .ToListAsync());
        Assert.Equal(1u, dan.DanId);
        Assert.Equal(20001u, dan.MedleyUniqueId);
        Assert.Equal(Ac15DanClearGrade.GoldClear, dan.ClearGrade);
    }

    private static TResponse Invoke<TController, TResponse>(
        TController controller,
        Func<TController, IActionResult> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        ConfigureController(controller);
        var ok = Assert.IsType<OkObjectResult>(action(controller));
        return assertResponse(ok.Value);
    }

    private static async Task<TResponse> InvokeAsync<TController, TResponse>(
        TController controller,
        Func<TController, Task<IActionResult>> action,
        Func<object?, TResponse> assertResponse)
        where TController : ControllerBase
    {
        ConfigureController(controller);
        var ok = Assert.IsType<OkObjectResult>(await action(controller));
        return assertResponse(ok.Value);
    }

    private static async Task<TResponse> InvokeMaybeAsync<TController, TResponse>(
        TController controller,
        Func<TController, object> action,
        Func<object?, TResponse> assertResponse,
        IServiceProvider services)
        where TController : ControllerBase
    {
        ConfigureController(controller, services);
        var result = action(controller);
        if (result is Task<IActionResult> task)
        {
            result = await task;
        }

        var ok = Assert.IsType<OkObjectResult>(result);
        return assertResponse(ok.Value);
    }

    private static void ConfigureController(ControllerBase controller, IServiceProvider? services = null)
        => controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = services ?? new ServiceCollection()
                    .AddLogging()
                    .BuildServiceProvider()
            }
        };

    private static ServiceProvider CreateServices(MurasakiHandlerFixture fixture)
        => new ServiceCollection()
            .AddApplication()
            .AddLogging()
            .AddSingleton(fixture.Context)
            .AddSingleton<ITaikoDbContext>(fixture.Context)
            .AddSingleton(fixture.Catalog)
            .BuildServiceProvider();

    private static MurasakiHandlerFixture.TestMurasakiCatalog CreateDanCatalog(params uint[] challengeLevels)
        => new(taikojukuFileOrder: challengeLevels
            .Select((dan, index) => new Ac15TaikojukuEntry
            {
                UniqueId = 20001u + (uint)index,
                ChallengeLevel = dan,
                DanLevel = dan,
                Name = $"Dan {dan}",
                Songs =
                [
                    new Ac15TaikojukuSong { SongNo = 101, Level = 1 },
                    new Ac15TaikojukuSong { SongNo = 102, Level = 1 }
                ]
            })
            .ToArray());

    private static FinalWire.PlayResultRequest CreateDanWireRequest(uint baid)
    {
        var request = new FinalWire.PlayResultRequest
        {
            Baid = baid,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            PlayDatetime = "20260623090000",
            IsRight = false,
            CardType = 1,
            IsTwoPlayers = false,
            GenderType = 0,
            PlayerAge = 0,
            PlayMode = (uint)PlayMode.DanMode,
            AreaCode = 1,
            Reserved = [],
            DanResult = (uint)Ac15DanClearGrade.GoldClear
        };
        request.AryStageInfoes.Add(CreateDanWireStage(101, score: 100000, soulGauge: 55));
        request.AryStageInfoes.Add(CreateDanWireStage(102, score: 200000, soulGauge: 88));
        return request;
    }

    private static FinalWire.PlayResultRequest.StageData CreateDanWireStage(
        uint songNo,
        uint score,
        uint soulGauge)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            PlayResult = 2,
            PlayScore = score,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            IsPapamama = false,
            PlayDan = 1,
            SoulGauge = soulGauge,
            HitCnt = 123,
            StageMode = 0,
            SelectedFolderId = 9
        };

    private static byte[] Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static List<int> ReadTopLevelFieldNumbers(byte[] protobuf)
    {
        var fields = new List<int>();
        var offset = 0;
        while (offset < protobuf.Length)
        {
            var tag = ReadVarint(protobuf, ref offset);
            fields.Add((int)(tag >> 3));
            var wireType = (int)(tag & 7);
            switch (wireType)
            {
                case 0:
                    _ = ReadVarint(protobuf, ref offset);
                    break;
                case 1:
                    offset += 8;
                    break;
                case 2:
                    var length = (int)ReadVarint(protobuf, ref offset);
                    offset += length;
                    break;
                case 5:
                    offset += 4;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported protobuf wire type {wireType}.");
            }
        }

        return fields;
    }

    private static ulong ReadVarint(byte[] buffer, ref int offset)
    {
        ulong result = 0;
        var shift = 0;
        while (offset < buffer.Length)
        {
            var b = buffer[offset++];
            result |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0)
            {
                return result;
            }

            shift += 7;
        }

        throw new InvalidOperationException("Unterminated protobuf varint.");
    }
}
