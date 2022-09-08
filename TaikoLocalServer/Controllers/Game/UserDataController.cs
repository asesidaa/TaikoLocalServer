using System.Buffers.Binary;
using System.Collections;
using System.Text.Json;
using Throw;

namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/userdata.php")]
[ApiController]
public class UserDataController : BaseController<UserDataController>
{
    private readonly TaikoDbContext context;
    
    public UserDataController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetUserData([FromBody] UserDataRequest request)
    {
        Logger.LogInformation("UserData request : {Request}", request.Stringify());

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

        var toneArray = new byte[16];
        Array.Fill(toneArray, byte.MaxValue);

        var recentSongs = context.SongPlayData
            .Where(datum => datum.Baid == request.Baid)
            .AsEnumerable()
            .OrderByDescending(datum => datum.PlayTime)
            .ThenByDescending(datum => datum.SongNumber)
            .Select(datum => datum.SongId)
            .ToArray();
        
        // Use custom implementation as distinctby cannot guarantee preserved element
        var recentSet = new OrderedSet<uint>();
        foreach (var id in recentSongs)
        {
            recentSet.Add(id);
            if (recentSet.Count == 10)
            {
                break;
            }
        }

        recentSongs = recentSet.ToArray();

        var userData = new UserDatum();
        if (context.UserData.Any(datum => datum.Baid == request.Baid))
        {
            userData = context.UserData.First(datum => datum.Baid == request.Baid);
        }

        var favoriteSongs = Array.Empty<uint>();
        try
        {
            favoriteSongs = JsonSerializer.Deserialize<uint[]>(userData.FavoriteSongsArray);
        }
        catch (JsonException e)
        {
            Logger.LogError(e, "Parsing favorite songs json data failed");
        }

        // The only way to get a null is provide string "null" as input,
        // which means database content need to be fixed, so better throw
        favoriteSongs.ThrowIfNull("Favorite song should never be null!");

        var defaultOptions = new byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(defaultOptions, userData.OptionSetting);
        
        var response = new UserDataResponse
        {
            Result = 1,
            ToneFlg = toneArray,
            // TitleFlg = GZipBytesUtil.GetGZipBytes(new byte[100]),
            ReleaseSongFlg = releaseSongArray,
            UraReleaseSongFlg = uraSongArray,
            DefaultOptionSetting = defaultOptions,
            IsVoiceOn = userData.IsVoiceOn,
            IsSkipOn = userData.IsSkipOn,
            IsChallengecompe = false,
            SongRecentCnt = (uint)recentSongs.Length,
            AryFavoriteSongNoes = favoriteSongs,
            AryRecentSongNoes = recentSongs,
            NotesPosition = userData.NotesPosition
        };

        return Ok(response);
    }
}