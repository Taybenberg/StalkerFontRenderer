namespace StalkerFontProcessing.Models;

internal record FontConfig
{
    public required int Height { get; init; }
    public required IDictionary<int, Character> Characters { get; init; }
}