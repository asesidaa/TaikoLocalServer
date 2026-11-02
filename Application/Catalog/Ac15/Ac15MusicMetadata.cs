using System.Text;

namespace TaikoLocalServer.Application.Catalog.Ac15;

public static class Ac15MusicMetadata
{
    public static uint MapGenreNameToCategoryId(string genreName)
    {
        var normalized = NormalizeFullWidthAscii(genreName).Trim().ToUpperInvariant();
        return normalized switch
        {
            "J-POP" or "POP" => (uint)SongGenre.Pop,
            "ANIME" or "アニメ" => (uint)SongGenre.Anime,
            "KIDS" or "DOYO" or "童謡" => (uint)SongGenre.Kids,
            "VOCALOID" or "ボーカロイド" => (uint)SongGenre.Vocaloid,
            "GAME" or "GAME MUSIC" or "GAMEMUSIC" or "ゲームミュージック" => (uint)SongGenre.GameMusic,
            "NAMCO" or "NAMCO ORIGINAL" or "NAMCOORIGINAL" or "ナムコオリジナル" => (uint)SongGenre.NamcoOriginal,
            "VARIETY" or "バラエティ" => (uint)SongGenre.Variety,
            "CLASSIC" or "CLASSICAL" or "クラシック" => (uint)SongGenre.Classical,
            _ => (uint)SongGenre.Pop
        };
    }

    public static string NormalizeFullWidthAscii(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            builder.Append(c switch
            {
                '\u3000' => ' ',
                >= '\uFF01' and <= '\uFF5E' => (char)(c - 0xFEE0),
                _ => c
            });
        }

        return builder.ToString();
    }
}
