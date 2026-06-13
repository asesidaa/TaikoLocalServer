using ProtoBuf;
using TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Red.Wire;
using RedV08R00 = TaikoLocalServer.Adapters.GameProtocol.Red.Wire.V08R00;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedProtocolMapperTests
{
    [Fact]
    public void BaidMapper_RedV08R00_UsesOlderBaidFieldNumbers()
    {
        var common = new CommonBaidResponse
        {
            Result = 1,
            Baid = 42,
            MyDonName = "DON",
            Title = "Title",
            TitlePlateId = 3,
            ColorFace = 4,
            ColorBody = 5,
            ColorLimb = 6,
            CostumeData = [1, 2, 3, 4, 5],
            GotDanMax = 12,
            GotDanFlg = new byte[Ac15EraProfiles.Red.Limits.DanFlagBytes],
            GotDanExtraFlg = new byte[Ac15EraProfiles.Red.Limits.DanExtraFlagBytes],
            DefaultToneSetting = 13,
            PersonId = "1"
        };

        var current = ApplyBaidControllerShape(BaidResponseMapper.Map(common));
        var older = ApplyBaidV08R00ControllerShape(BaidResponseMapper.MapV08R00(common));
        var currentFields = ReadLengthDelimitedFieldLengths(Serialize(current));
        var olderFields = ReadLengthDelimitedFieldLengths(Serialize(older));

        Assert.Contains(Ac15EraProfiles.Red.Limits.DanExtraFlagBytes, currentFields[30]);
        Assert.False(olderFields.ContainsKey(30));
        Assert.Contains(Ac15EraProfiles.Red.Limits.ContentInfoBytes, olderFields[31]);
        Assert.Contains(1, olderFields[33]);
        Assert.Contains(Ac15EraProfiles.Red.Limits.ContentInfoBytes, currentFields[32]);
        Assert.Contains(1, currentFields[34]);
    }

    [Fact]
    public void PlayResultMapper_Red_MapsDonPointTokkunAndChallengeFacts()
    {
        var request = CreateWireRequest(1);
        request.GetDonpoint = 25;
        request.RewardPtn = 4;
        request.RewardProgress = 9;
        request.DifficultyTutorialFlg = 2;
        request.DifficultyPlayedCourse = 3;
        request.DifficultyPlayedStar = 8;
        request.TokkunTutorialFlg = 7;
        request.AryTokkunstageInfo = new PlayResultRequest.TokkunstageData
        {
            BanacoinDatetime = "20260608120100",
            TokkunSongCnt = 3,
            TookunSongnoes = [101, 102, 101],
            TokkunSpeedchangeCnt = 3,
            TokkunAutoplayCnt = 4,
            TokkunJumpCnt = 5
        };
        var stage = new PlayResultRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            PlayResult = 2,
            PlayScore = 765432,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            OptionFlg = [1],
            ToneFlg = [2],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            IsPapamama = false,
            PlayDan = 1,
            SoulGauge = 80,
            HitCnt = 123,
            StageMode = 0,
            SelectedFolderId = 9
        };
        stage.AryChallengeIds.Add(new PlayResultRequest.StageData.ResultcompeData { CompeId = 42, TrackNo = 2 });
        request.AryStageInfoes.Add(stage);

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(25u, envelope.Profile.GetDonpoint);
        Assert.Equal(4u, envelope.Profile.RewardPtn);
        Assert.Equal(9u, envelope.Profile.RewardProgress);
        Assert.Equal(2u, envelope.Profile.DifficultyTutorialFlg);
        Assert.True(envelope.Profile.HasDifficultyPlayedCourse);
        Assert.True(envelope.Profile.HasDifficultyPlayedStar);
        Assert.Equal(3u, envelope.Profile.DifficultyPlayedCourse);
        Assert.Equal(8u, envelope.Profile.DifficultyPlayedStar);
        Assert.NotNull(envelope.Tokkun);
        Assert.Equal(7u, envelope.Tokkun!.TutorialFlg);
        Assert.NotNull(envelope.Tokkun.StageData);
        Assert.Equal([101u, 102u, 101u], envelope.Tokkun.StageData!.TookunSongnoes);
        var stageResult = Assert.Single(envelope.Normal!.Stages);
        Assert.Equal(42u, Assert.Single(stageResult.ChallengeIds).CompeId);
        Assert.Equal(1u, stageResult.PlayDan);
        Assert.Equal(80u, stageResult.SoulGauge);
        Assert.NotNull(envelope.ChallengeCompe);
    }

    [Fact]
    public void UserDataMapper_Red_MapsDonPointFieldsToRedWire()
    {
        var response = UserDataMappers.Map(new CommonUserDataResponse
        {
            Result = 1,
            ReleaseSongFlg = [1, 2],
            ToneFlg = [3],
            TitleFlg = [4],
            AryFavoriteSongNoes = [101],
            AryRecentSongNoes = [102],
            TotalGetDonpoint = 120,
            TotalUseDonpoint = 30,
            RewardProgress = 8,
            DifficultyTutorialFlg = 2,
            TokkunTutorialFlg = 7,
            IsDevilRed = true,
            IsExplainRed = true,
            DispTaikojukuDan = 1,
            RecommendBestSong = [103]
        });

        Assert.Equal(120u, response.TotalGetDonpoint);
        Assert.Equal(30u, response.TotalUseDonpoint);
        Assert.Equal(8u, response.RewardProgress);
        Assert.Equal(2u, response.DifficultyTutorialFlg);
        Assert.Equal(7u, response.TokkunTutorialFlg);
        Assert.True(response.IsDevil);
        Assert.True(response.IsExplain);
        Assert.Equal([101u], response.AryFavoriteSongNoes);
        Assert.Equal([102u], response.AryRecentSongNoes);
        Assert.Equal([103u], response.RecommendBestSongs);
    }

    private static PlayResultRequest CreateWireRequest(uint baid)
        => new()
        {
            Baid = baid,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            PlayDatetime = "20260608120000",
            IsRight = false,
            CardType = 1,
            IsTwoPlayers = false,
            GenderType = 0,
            PlayerAge = 0,
            PlayMode = (uint)PlayMode.Normal,
            AreaCode = 1,
            Reserved = new byte[16]
        };

    private static BAIDResponse ApplyBaidControllerShape(BAIDResponse response)
    {
        response.AccessCode = "12345678901234567890";
        response.IsPublish = true;
        response.PlayerType = 0;
        response.ComSvrResult = 1;
        response.Personid = "1";
        response.RegCountryId = "JPN";
        response.MbId = 1;
        response.PurposeId = 1;
        response.RegionId = 1;
        return response;
    }

    private static RedV08R00.BAIDResponse ApplyBaidV08R00ControllerShape(RedV08R00.BAIDResponse response)
    {
        response.AccessCode = "12345678901234567890";
        response.IsPublish = true;
        response.PlayerType = 0;
        response.ComSvrResult = 1;
        response.Personid = "1";
        response.RegCountryId = "JPN";
        response.MbId = 1;
        response.PurposeId = 1;
        response.RegionId = 1;
        return response;
    }

    private static byte[] Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static Dictionary<int, List<int>> ReadLengthDelimitedFieldLengths(byte[] payload)
    {
        var result = new Dictionary<int, List<int>>();
        var index = 0;
        while (index < payload.Length)
        {
            var key = ReadVarint(payload, ref index);
            var fieldNumber = (int)(key >> 3);
            var wireType = (int)(key & 0b111);

            switch (wireType)
            {
                case 0:
                    ReadVarint(payload, ref index);
                    break;
                case 1:
                    index += 8;
                    break;
                case 2:
                    var length = (int)ReadVarint(payload, ref index);
                    if (!result.TryGetValue(fieldNumber, out var lengths))
                    {
                        lengths = [];
                        result[fieldNumber] = lengths;
                    }
                    lengths.Add(length);
                    index += length;
                    break;
                case 5:
                    index += 4;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported protobuf wire type {wireType}.");
            }
        }

        return result;
    }

    private static ulong ReadVarint(byte[] payload, ref int index)
    {
        ulong value = 0;
        var shift = 0;
        while (index < payload.Length)
        {
            var current = payload[index++];
            value |= (ulong)(current & 0x7F) << shift;
            if ((current & 0x80) == 0)
            {
                return value;
            }

            shift += 7;
        }

        throw new InvalidOperationException("Unterminated protobuf varint.");
    }
}
