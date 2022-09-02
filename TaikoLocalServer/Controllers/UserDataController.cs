using System.Collections;
using System.Text.Json;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/userdata.php")]
[ApiController]
public class UserDataController : ControllerBase
{
    private readonly ILogger<UserDataController> logger;
    
    private readonly TaikoDbContext context;
    
    public UserDataController(ILogger<UserDataController> logger, TaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetUserData([FromBody] UserDataRequest request)
    {
        logger.LogInformation("UserData request : {Request}", request.Stringify());

        var musicAttributeManager = MusicAttributeManager.Instance;

        var releaseSongArray = new byte[Constants.MUSIC_FLAG_ARRAY_SIZE];
        var bitSet = new BitArray(Constants.MUSIC_ID_MAX);
        foreach (var music in musicAttributeManager.Musics)
        {
            bitSet.Set((int)music, true);
        }
        bitSet.CopyTo(releaseSongArray, 0); 
        
        var uraSongArray = new byte[Constants.MUSIC_FLAG_ARRAY_SIZE];
        bitSet.SetAll(false);
        foreach (var music in musicAttributeManager.MusicsWithUra)
        {
            bitSet.Set((int)music, true);
        }
        bitSet.CopyTo(uraSongArray, 0);

        var toneArray = new byte[5];
        Array.Fill(toneArray, byte.MaxValue);

        var recentSongs = context.SongPlayData
            .Where(datum => datum.Baid == request.Baid)
            .AsEnumerable()
            .DistinctBy(datum => datum.SongId)
            .OrderByDescending(datum => datum.PlayTime)
            .ThenByDescending(datum => datum.SongNumber)
            .Select(datum => datum.SongId)
            .Take(10)
            .ToArray();

        var userData = new UserDatum();
        if (context.UserData.Any(datum => datum.Baid == request.Baid))
        {
            userData = context.UserData.First(datum => datum.Baid == request.Baid);
        }

        var favoriteSongs = new uint[] { 0 };
        try
        {
            favoriteSongs = JsonSerializer.Deserialize<uint[]>(userData.FavoriteSongsArray);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Parsing favorite songs json data failed");
        }
        if (favoriteSongs == null || favoriteSongs.Length < 1)
        {
            logger.LogWarning("Favorite songs is null!");
            favoriteSongs = new uint[] { };
        }

        var response = new UserDataResponse
        {
            Result = 1,
            ToneFlg = toneArray,
            // TitleFlg = GZipBytesUtil.GetGZipBytes(new byte[100]),
            ReleaseSongFlg = releaseSongArray,
            UraReleaseSongFlg = uraSongArray,
            DefaultOptionSetting = new byte[] {0},
            IsVoiceOn = userData.IsVoiceOn,
            IsSkipOn = userData.IsSkipOn,
            IsChallengecompe = false,
            SongRecentCnt = (uint)recentSongs.Length,
            AryFavoriteSongNoes = favoriteSongs,
            AryRecentSongNoes = recentSongs
        };
        
        

        return Ok(response);
    }
}