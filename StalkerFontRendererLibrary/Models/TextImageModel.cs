using SixLabors.ImageSharp;

namespace StalkerFontRendererLibrary.Models;

public record TextImageModel
{
    public required string Text { get; init; }

    public required int ImageWidth { get; init; }
    public required int ImageHeight { get; init; }

    public required int LineHeight { get; init; }
    public required int CharacterSpacing { get; init; }

    public byte A { get; init; }
    public byte R { get; init; }
    public byte G { get; init; }
    public byte B { get; init; }

    internal Color GetColor() => Color.FromRgba(R, G, B, A);
}