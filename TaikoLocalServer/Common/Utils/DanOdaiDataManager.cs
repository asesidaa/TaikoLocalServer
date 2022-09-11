using System.Collections.Immutable;
using System.Text.Json;
using SharedProject.Models;
using Swan.Mapping;

namespace TaikoLocalServer.Common.Utils;

public class DanOdaiDataManager
{
    public ImmutableDictionary<uint, GetDanOdaiResponse.OdaiData> OdaiDataList { get; }
    
    static DanOdaiDataManager() {}

    private DanOdaiDataManager()
    {
        var dataPath = PathHelper.GetDataPath();
        var filePath = Path.Combine(dataPath, Constants.DAN_DATA_FILE_NAME);
        var jsonString = File.ReadAllText(filePath);
        
        var result = JsonSerializer.Deserialize<List<DanData>>(jsonString);

        if (result is null)
        {
            throw new ApplicationException("Cannot parse dan data json!");
        }

        OdaiDataList = result.ToImmutableDictionary(data => data.DanId, ToResponseOdaiData);
    }
    private GetDanOdaiResponse.OdaiData ToResponseOdaiData(DanData data)
    {
        var responseOdaiData = new GetDanOdaiResponse.OdaiData
        {
            DanId = data.DanId,
            Title = data.Title,
            VerupNo = data.VerupNo
        };

        var odaiSongs = data.OdaiSongList.Select(song => song.CopyPropertiesToNew<GetDanOdaiResponse.OdaiData.OdaiSong>());
        responseOdaiData.AryOdaiSongs.AddRange(odaiSongs);

        var odaiBorders = data.OdaiBorderList.Select(border => border.CopyPropertiesToNew<GetDanOdaiResponse.OdaiData.OdaiBorder>());
        responseOdaiData.AryOdaiBorders.AddRange(odaiBorders);

        return responseOdaiData;
    }

    public static DanOdaiDataManager Instance { get; } = new();
}