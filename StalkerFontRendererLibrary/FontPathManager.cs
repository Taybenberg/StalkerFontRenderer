namespace StalkerFontProcessing;

public class FontPathManager
{
    private const string TextureExtension = ".dds";
    private const string ConfigurationExtension = ".ini";

    private readonly string _fontsPath;
    public FontPathManager(string path)
    {
        _fontsPath = path;

        var files = Directory.GetFiles(_fontsPath)
                .Select(x => Path.GetFileNameWithoutExtension(x))
                .Distinct()
                .Where(x => x.Contains("font"))
                .OrderBy(x => x)
                .ToList();

        FontNames = files.Where(x => IsFontPathValid(Path.Combine(_fontsPath, x))).ToArray();
    }

    public IEnumerable<string> FontNames { get; }

    public string GetFullFontPath(string fontName)
        => Path.Combine(_fontsPath, fontName);

    public static bool IsFontPathValid(string path)
    {
        return File.Exists(Path.ChangeExtension(path, TextureExtension))
            && File.Exists(Path.ChangeExtension(path, ConfigurationExtension));
    }

    public static string GetTexturePath(string path)
        => Path.ChangeExtension(path, TextureExtension);

    public static string GetConfigurationPath(string path)
        => Path.ChangeExtension(path, ConfigurationExtension);
}
