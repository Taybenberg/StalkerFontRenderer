using StalkerFontProcessing;
using StalkerFontRenderer.Events;
using StalkerFontRendererLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StalkerFontRenderer.Services;

class FontService : IDisposable
{
    private FontRenderer? _fontRenderer;
    private FontPathManager _fontPathManager;
    public FontService(string fontsPath)
    {
        _fontPathManager = new(fontsPath);
    }

    public event EventHandler<ImageSourceEventArgs>? FontTextureUpdated;
    public event EventHandler<ImageSourceEventArgs>? TextImageUpdated;
    public event EventHandler<EventArgs>? ImageNotCompleted;

    public IEnumerable<string> FontNames => _fontPathManager.FontNames;
    public int LineHeight => _fontRenderer!.FontHeight;

    public string? ImageText { get; private set; }
    public int TextImageWidth { get; private set; }
    public int TextImageHeight { get; private set; }
    public int TextImageLineHeight { get; private set; }
    public int TextImageCharacterSpacing { get; private set; }

    public Color BackgroundColor { get; private set; }
    public Color TextColor { get; private set; }

    public void Dispose()
    {
        _fontRenderer?.Dispose();
        _fontRenderer = null;
    }

    public Task SetFontAsync(string fontName)
    {
        _fontRenderer?.Dispose();
        _fontRenderer = new(_fontPathManager.GetFullFontPath(fontName));

        return OnSetTextColorAsync();
    }

    public Task SetTextColorAsync(Color color)
    {
        TextColor = color;
        return OnSetTextColorAsync();
    }

    public Task SetBackgroundColorAsync(Color color)
    {
        BackgroundColor = color;
        return Task.CompletedTask;
    }

    public Task SetImageTextAsync(string? text, bool forcedUpdate = false)
    {
        ImageText = text;
        return OnUpdatedTextImagePropertyAsync(forcedUpdate);
    }

    public Task SetTextImageWidthAsync(int width)
    {
        TextImageWidth = width;
        return OnUpdatedTextImagePropertyAsync();
    }

    public Task SetTextImageHeightAsync(int height)
    {
        TextImageHeight = height;
        return OnUpdatedTextImagePropertyAsync();
    }

    public Task SetTextImageLineHeightAsync(int lineHeight)
    {
        TextImageLineHeight = lineHeight;
        return OnUpdatedTextImagePropertyAsync();
    }

    public Task SetTextImageCharacterSpacingAsync(int characterSpacing)
    {
        TextImageCharacterSpacing = characterSpacing;
        return OnUpdatedTextImagePropertyAsync();
    }

    public async Task<Stream> RenderTextImageAsync(bool useTransparentBackground)
    {
        var model = new TextImageModel
        {
            Text = ImageText ?? string.Empty,
            ImageHeight = TextImageHeight,
            ImageWidth = TextImageWidth,
            LineHeight = TextImageLineHeight,
            CharacterSpacing = TextImageCharacterSpacing,
        };

        if (!useTransparentBackground)
        {
            model = model with
            { 
                A = BackgroundColor.A, 
                R = BackgroundColor.R, 
                G = BackgroundColor.G, 
                B = BackgroundColor.B 
            };
        }

        var result = await _fontRenderer!.GetRenderedImageStreamAsync(model).ConfigureAwait(false);

        if (!result.IsCompleted)
        {
            ImageNotCompleted?.Invoke(this, new());
        }

        return result.Result;
    }

    private async Task OnSetTextColorAsync()
    {
        if (_fontRenderer is not null)
        {
            _fontRenderer.SetTextColor(TextColor.R, TextColor.G, TextColor.B);
            await OnFontTextureUpdatedAsync().ConfigureAwait(false);
            await OnUpdatedTextImagePropertyAsync().ConfigureAwait(false);
        }
    }

    private Task OnUpdatedTextImagePropertyAsync(bool forced = false)
    {
        if (_fontRenderer is not null && (forced || !string.IsNullOrEmpty(ImageText)))
        {
            return OnTextImageUpdatedAsync();
        }
        return Task.CompletedTask;
    }

    private async Task OnTextImageUpdatedAsync()
    {
        var stream = await RenderTextImageAsync(true).ConfigureAwait(false);
        var imageSource = GetImageSource(stream);

        TextImageUpdated?.Invoke(this, new(imageSource));
    }

    private async Task OnFontTextureUpdatedAsync()
    {
        var stream = await _fontRenderer!.GetFontTextureStreamAsync().ConfigureAwait(false);
        var imageSource = GetImageSource(stream);

        FontTextureUpdated?.Invoke(this, new(imageSource));
    }

    private ImageSource GetImageSource(Stream stream)
    {
        var imageSource = new BitmapImage();

        imageSource.BeginInit();
        imageSource.StreamSource = stream;
        imageSource.EndInit();

        return imageSource;
    }
}
