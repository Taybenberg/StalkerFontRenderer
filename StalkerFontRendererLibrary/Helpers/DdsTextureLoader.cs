using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace StalkerFontProcessing.Helpers;

internal static class DdsTextureLoader
{
    public static Image<Rgba32> LoadTexture(string path)
    {
        var texture = Pfimage.FromFile(FontPathManager.GetTexturePath(path));

        if (texture.Compressed)
        {
            texture.Decompress();
        }

        texture.ApplyColorMap();

        var bytes = GetTextureBytes(texture);

        using Image image = texture.Format is ImageFormat.Rgba32
            ? Image.LoadPixelData<Rgba32>(bytes, texture.Width, texture.Height)
            : Image.LoadPixelData<A8>(bytes, texture.Width, texture.Height);

        var clone = image.CloneAs<Rgba32>();

        if (texture.Format is not ImageFormat.Rgba32)
        {
            clone.Mutate(m => m.Invert());
        }

        return clone;
    }

    private static byte[] GetTextureBytes(Pfim.IImage image)
    {
        var tightStride = image.Width * image.BitsPerPixel / 8;
        if (image.Stride == tightStride)
        {
            return image.Data;
        }

        var bytes = new byte[image.Height * tightStride];
        for (int i = 0; i < image.Height; i++)
        {
            Buffer.BlockCopy(image.Data, i * image.Stride, bytes, i * tightStride, tightStride);
        }

        return bytes;
    }
}