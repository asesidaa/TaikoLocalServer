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
        var common = new Ac15BaidResponse
        {
            Result = 1,
            Baid = 42,
            Identity = new Ac15BaidIdentity("DON", 0),
            MydonProfile = new Ac15BaidProfile
            {
                Title = "Title",
                TitlePlateId = 3,
                ColorFace = 4,
                ColorBody = 5,
                ColorLimb = 6,
                SelectedCostume = new Ac15CostumeFacts(1, 2, 3, 4, 5),
                DefaultToneSetting = 13
            },
            DanStatus = new Ac15BaidDan(
                0,
                12,
                new byte[Ac15EraProfiles.Red.Limits.DanFlagBytes],
                new byte[Ac15EraProfiles.Red.Limits.DanExtraFlagBytes]),
            CompatibilityProfile = new Ac15BaidCompatibility("1", null)
        };

        var current = AssembleBaidControllerShape(common);
        var older = AssembleBaidV08R00ControllerShape(common);
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
    public void PlayResultMapper_Red_MapsDonPointTokkunAndProtocolChallengeFactsOnStages()
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
    }

    [Fact]
    public void UserDataMapper_Red_MapsDonPointFieldsToRedWire()
    {
        var response = AssembleRedUserDataResponse(new Ac15UserDataResponse
        {
            Result = 1,
            SongFlags = new Ac15UserDataSongFlags
            {
                ReleaseSongFlg = [1, 2],
                ToneFlg = [3],
                TitleFlg = [4]
            },
            SongLists = new Ac15UserDataSongLists
            {
                AryFavoriteSongNoes = [101],
                AryRecentSongNoes = [102]
            },
            Reward = new Ac15UserDataReward(120, 30, 8),
            Tutorial = new Ac15UserDataTutorial(7, 2),
            ModeFlags = new Ac15UserDataModeFlags(true, true),
            Display = new Ac15UserDataDisplaySettings { DispTaikojukuDan = 1 },
            Recommendations = new Ac15UserDataRecommendations { RecommendBestSong = [103] }
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

    [Fact]
    public void ChallengeCompeMapper_Red_MapsActiveChallengeProgressAndKeepsUnsupportedBucketsEmpty()
    {
        var response = ChallengeCompeMappers.Map(new CommonChallengeCompeResponse
        {
            Result = 1,
            AryChallengeStat =
            [
                new CommonChallengeCompeResponse.CompeData
                {
                    CompeId = 1001,
                    AryTrackStat =
                    [
                        new CommonChallengeCompeResponse.TracksData
                        {
                            SongNo = 101,
                            Level = 1,
                            OptionFlg = [1, 2, 3],
                            StageMode = 0,
                            HighScore = 800000
                        }
                    ]
                }
            ]
        });

        Assert.Equal(1u, response.Result);
        var challenge = Assert.Single(response.AryChallengeStats);
        Assert.Equal(1001u, challenge.CompeId);
        var track = Assert.Single(challenge.AryTrackStats);
        Assert.Equal(101u, track.SongNo);
        Assert.Equal(1u, track.Level);
        Assert.Equal([1, 2, 3], track.OptionFlg);
        Assert.Equal(0u, track.StageMode);
        Assert.Equal(800000u, track.HighScore);
        Assert.Empty(response.AryUserCompeStats);
        Assert.Empty(response.AryBngCompeStats);
    }

    private static UserDataResponse AssembleRedUserDataResponse(Ac15UserDataResponse common)
    {
        var response = new UserDataResponse
        {
            Result = common.Result
        };

        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Recommendations, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            UserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            UserDataMappers.Apply(tutorial, response);
        }

        if (common.Reward is { } reward)
        {
            UserDataMappers.Apply(reward, response);
        }

        return response;
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

    private static BAIDResponse AssembleBaidControllerShape(Ac15BaidResponse common)
    {
        var response = new BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            ContentInfo = new byte[Ac15EraProfiles.Red.Limits.ContentInfoBytes]
        };

        if (common.Identity is { } identity)
        {
            BaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            BaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            BaidResponseMapper.Apply(inventory, response);
        }

        if (common.DanStatus is { } dan)
        {
            BaidResponseMapper.Apply(dan, response);
        }

        if (common.CompatibilityProfile is { } compatibility)
        {
            BaidResponseMapper.Apply(compatibility, response);
        }

        if (common.RewardProgress is { } reward)
        {
            BaidResponseMapper.Apply(reward, response);
        }

        return ApplyBaidControllerShape(response);
    }

    private static RedV08R00.BAIDResponse AssembleBaidV08R00ControllerShape(Ac15BaidResponse common)
    {
        var response = new RedV08R00.BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            ContentInfo = new byte[Ac15EraProfiles.Red.Limits.ContentInfoBytes]
        };

        if (common.Identity is { } identity)
        {
            BaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            BaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            BaidResponseMapper.Apply(inventory, response);
        }

        if (common.DanStatus is { } dan)
        {
            BaidResponseMapper.Apply(dan, response);
        }

        if (common.CompatibilityProfile is { } compatibility)
        {
            BaidResponseMapper.Apply(compatibility, response);
        }

        if (common.RewardProgress is { } reward)
        {
            BaidResponseMapper.Apply(reward, response);
        }

        return ApplyBaidV08R00ControllerShape(response);
    }

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
