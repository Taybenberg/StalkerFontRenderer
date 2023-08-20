namespace StalkerFontRendererLibrary.Models;

public record RenderResultModel
{
    public required Stream Result { get; init; }
    public required bool IsCompleted { get; init; }
}
