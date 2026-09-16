using System.Text.Json;
using CladTracker.Models;

namespace CladTracker.Services;

public sealed class EntryStore
{
    public const decimal RatePerGram = 350m;

    private readonly string filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CladTracker",
        "entries.json");

    private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

    public IReadOnlyList<CladEntry> Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<CladEntry>>(
                File.ReadAllText(filePath), jsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
    }

    public void Save(IEnumerable<CladEntry> entries)
    {
        var directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = filePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(entries, jsonOptions));
        File.Move(temporaryPath, filePath, true);
    }
}
