using System.Text.Json;

namespace Khaos.JEX.LanguageServer.Services;

/// <summary>
/// Loads function manifests that describe host-registered C# functions.
/// </summary>
public sealed class FunctionManifestLoader
{
    private readonly List<ManifestFunction> _functions = new();
    private readonly HashSet<string> _loadedFiles = new();

    /// <summary>
    /// Gets all loaded manifest functions.
    /// </summary>
    public IReadOnlyList<ManifestFunction> Functions => _functions;

    /// <summary>
    /// Loads a function manifest from a JSON file.
    /// </summary>
    public void LoadManifest(string filePath)
    {
        if (_loadedFiles.Contains(filePath)) return;

        try
        {
            var json = File.ReadAllText(filePath);
            var manifest = JsonSerializer.Deserialize<FunctionManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (manifest?.Functions is not null)
            {
                _functions.AddRange(manifest.Functions);
                _loadedFiles.Add(filePath);
            }
        }
        catch
        {
            // Silently ignore invalid manifests
        }
    }

    /// <summary>
    /// Loads all manifests from a directory (*.jex.functions.json).
    /// </summary>
    public void LoadManifestsFromDirectory(string directory)
    {
        if (!Directory.Exists(directory)) return;

        foreach (var file in Directory.GetFiles(directory, "*.jex.functions.json", SearchOption.AllDirectories))
        {
            LoadManifest(file);
        }
    }

    /// <summary>
    /// Clears all loaded functions.
    /// </summary>
    public void Clear()
    {
        _functions.Clear();
        _loadedFiles.Clear();
    }

    /// <summary>
    /// Gets a function by name.
    /// </summary>
    public ManifestFunction? GetFunction(string name)
    {
        return _functions.FirstOrDefault(f =>
            string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}

public class FunctionManifest
{
    public List<ManifestFunction>? Functions { get; set; }
}

public class ManifestFunction
{
    public string Name { get; set; } = "";
    public string? Signature { get; set; }
    public string? Description { get; set; }
    public int MinArgs { get; set; }
    public int MaxArgs { get; set; } = int.MaxValue;
    public List<ManifestParameter>? Parameters { get; set; }
}

public class ManifestParameter
{
    public string Name { get; set; } = "";
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool Optional { get; set; }
}
