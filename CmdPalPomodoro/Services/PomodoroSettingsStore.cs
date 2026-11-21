using System.Text.Json;
using System.Text.Json.Serialization;

namespace CmdPalPomodoro.Services;

internal static class PomodoroSettingsStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CmdPalPomodoro",
        "settings.json");

    public static PomodoroSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new PomodoroSettings();
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize(json, PomodoroJsonContext.Default.PomodoroSettings) ?? new PomodoroSettings();
        }
        catch (IOException)
        {
            return new PomodoroSettings();
        }
        catch (JsonException)
        {
            return new PomodoroSettings();
        }
    }

    public static void Save(PomodoroSettings settings)
    {
        var directory = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(settings, PomodoroJsonContext.Default.PomodoroSettings);
        var temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, json);
        File.Move(temporary, FilePath, overwrite: true);
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PomodoroSettings))]
internal sealed partial class PomodoroJsonContext : JsonSerializerContext;
