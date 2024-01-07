using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StalkerFontProcessing.Helpers;
using StalkerFontProcessing.Models;
using StalkerFontRendererLibrary.Models;

namespace StalkerFontProcessing;

public class FontRenderer(string path) : IDisposable
{
    private readonly Image<Rgba32> _fontTexture = DdsTextureLoader.LoadTexture(path);
    private readonly FontConfig _fontConfig = FontConfigLoader.LoadFontConfig(path);

    private bool _disposed;

    public int FontHeight => _fontConfig.Height;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _fontTexture.Dispose();
            }

            _disposed = true;
        }
    }

    public void SetTextColor(byte r, byte g, byte b)
    {
        for (int i = 0; i < _fontTexture.Width; i++)
        {
            for (int j = 0; j < _fontTexture.Height; j++)
            {
                var pixel = _fontTexture[i, j];

                pixel.R = r;
                pixel.G = g;
                pixel.B = b;

                _fontTexture[i, j] = pixel;
            }
        }
    }

    public Task<Stream> GetFontTextureStreamAsync()
        => GetImageStreamAsync(_fontTexture);

    public async Task<RenderResultModel> GetRenderedImageStreamAsync(TextImageModel model)
    {
        using var image = RenderTextImage(model, out var isCompleted);

        var stream = await GetImageStreamAsync(image).ConfigureAwait(false);

        return new RenderResultModel
        {
            IsCompleted = isCompleted,
            Result = stream
        };
    }

    private Image<Rgba32> RenderTextImage(TextImageModel model, out bool isCompleted)
    {
        var image = new Image<Rgba32>(model.ImageWidth, model.ImageHeight, model.GetColor());

        bool flag = true;

        if (!string.IsNullOrEmpty(model.Text))
        {
            var message = TextHelper.EncodeText(model.Text);

            int x = 0, y = model.LineHeight;

            image.Mutate(m =>
            {
                foreach (var c in message)
                {
                    if (c is (int)'\n')
                    {
                        y += model.LineHeight;
                        x = 0;

                        continue;
                    }

                    if (!_fontConfig.Characters.ContainsKey(c))
                    {
                        continue;
                    }

                    using var character = GetCharacter(_fontConfig.Characters[c]);
                    try
                    {
                        m.DrawImage(character, new Point(x, y - character.Height), 1);
                    }
                    catch
                    {
                        flag = false;
                        return;
                    }

                    x += character.Width + model.CharacterSpacing;
                }
            });
        }

        isCompleted = flag;

        return image;
    }

    private Image<Rgba32> GetCharacter(Character character)
        => _fontTexture.Clone(m =>
        {
            m.Crop(new Rectangle(character.X, character.Y, character.Width, character.Height));
        });

    private static async Task<Stream> GetImageStreamAsync(Image image)
    {
        var stream = new MemoryStream();

        await image.SaveAsPngAsync(stream).ConfigureAwait(false);

        stream.Position = 0;

        return stream;
    }
}