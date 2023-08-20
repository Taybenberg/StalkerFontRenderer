using StalkerFontProcessing.Models;
using System.Text.RegularExpressions;

namespace StalkerFontProcessing.Helpers;

internal static class FontConfigLoader
{
    public static FontConfig LoadFontConfig(string path)
    {
        var characterRegex = new Regex("^(\\d+)\\s*=\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+)");
        var heightRegex = new Regex("^height\\s*=\\s*(\\d+)");

        var lines = File.ReadAllLines(FontPathManager.GetConfigurationPath(path));

        int? height = null;
        var dictionary = new Dictionary<int, Character>();

        foreach (var line in lines)
        {
            var match = characterRegex.Match(line);

            if (match.Success)
            {
                var code = int.Parse(match.Groups[1].Value);

                var x = int.Parse(match.Groups[2].Value);
                var y = int.Parse(match.Groups[3].Value);
                var w = int.Parse(match.Groups[4].Value) - x;
                var h = int.Parse(match.Groups[5].Value) - y;

                dictionary.Add(code, new()
                {
                    X = x,
                    Y = y,
                    Width = w,
                    Height = h,
                });
            }
            else if (height is null)
            {
                var heightMatch = heightRegex.Match(line);

                if (heightMatch.Success)
                {
                    height = int.Parse(heightMatch.Groups[1].Value);
                }
            }
        }

        return new()
        {
            Height = height ?? dictionary.Last().Value.Height,
            Characters = dictionary,
        };
    }
}
