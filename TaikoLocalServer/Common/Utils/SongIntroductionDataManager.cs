using System.Collections.Immutable;
using System.Text.Json;
using SharedProject.Models;

namespace TaikoLocalServer.Common.Utils;

public class SongIntroductionDataManager
{
    public ImmutableDictionary<uint, GetSongIntroductionResponse.SongIntroductionData> IntroDataList { get; }

    static SongIntroductionDataManager() { }

    private SongIntroductionDataManager()
    {
        var dataPath = PathHelper.GetDataPath();
        var filePath = Path.Combine(dataPath, Constants.INTRO_DATA_FILE_NAME);
        var jsonString = File.ReadAllText(filePath);

        var result = JsonSerializer.Deserialize<List<SongIntroductionData>>(jsonString);

        if (result is null)
        {
            throw new ApplicationException("Cannot parse intro data json!");
        }

        IntroDataList = result.ToImmutableDictionary(data => data.SetId, ToResponseIntroData);
    }
    private GetSongIntroductionResponse.SongIntroductionData ToResponseIntroData(SongIntroductionData data)
    {
        var responseOdaiData = new GetSongIntroductionResponse.SongIntroductionData
        {
            SetId = data.SetId,
            VerupNo = data.VerupNo,
            MainSongNo = data.MainSongNo,
            SubSongNoes = data.SubSongNo
        };

        return responseOdaiData;
    }

    public static SongIntroductionDataManager Instance { get; } = new();
}