namespace TaikoLocalServer.Infrastructure.Extract.Textures;

/// <summary>An 8-bit RGBA image, rows top to bottom.</summary>
public sealed class RgbaImage(int width, int height, byte[]? pixels = null)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public byte[] Pixels { get; } = pixels ?? new byte[width * height * 4];

    public Span<byte> Pixel(int x, int y) => Pixels.AsSpan((y * Width + x) * 4, 4);

    /// <summary>Copies <paramref name="image"/> in at (<paramref name="x"/>, <paramref name="y"/>), clipped to this image.</summary>
    public void Paste(RgbaImage image, int x, int y)
    {
        for (var row = 0; row < image.Height; row++)
        {
            var ty = y + row;
            if (ty < 0 || ty >= Height) continue;
            var x0 = Math.Max(0, x);
            var x1 = Math.Min(Width, x + image.Width);
            if (x1 <= x0) continue;
            image.Pixels.AsSpan((row * image.Width + (x0 - x)) * 4, (x1 - x0) * 4).CopyTo(Pixels.AsSpan((ty * Width + x0) * 4));
        }
    }

    /// <summary>Images of one width stacked top to bottom (a face's 12 expressions).</summary>
    public static RgbaImage Stack(IReadOnlyList<RgbaImage> images)
    {
        var w = images[0].Width;
        var stacked = new RgbaImage(w, images.Sum(i => i.Height));
        var y = 0;
        foreach (var image in images)
        {
            stacked.Paste(image, 0, y);
            y += image.Height;
        }
        return stacked;
    }

    public void SavePng(string path) => File.WriteAllBytes(path, Png.Encode(this));
}
