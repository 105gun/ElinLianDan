using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LianDan;

internal record ModSettings
{
    public bool legacyRatio = false;
    public float extraMultiplier = 1.0f;
}

internal class Settings
{
    static string currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    static string settingsPath {
        get {
            return Path.Combine(currentPath, "settings.json");
        }
    }
    private static ModSettings settings = null;

    public static ModSettings GetSettings()
    {
        if (settings == null)
        {
            LoadSettings();
        }
        return settings;
    }

    public static void LoadSettings()
    {
        if (!System.IO.File.Exists(settingsPath))
        {
            Plugin.ModLog("Settings file not found, using default values", PrivateLogLevel.Info);
            settings = new ModSettings();
            SaveSettings();
            return;
        }
        try 
        {
            string json = System.IO.File.ReadAllText(settingsPath);
            settings = JsonConvert.DeserializeObject<ModSettings>(json);
            SaveSettings();
        }
        catch (JsonReaderException)
        {
            Plugin.ModLog("Failed to load settings, using default values", PrivateLogLevel.Warning);
            settings = new ModSettings();
            SaveSettings();
        }
    }

    public static void SaveSettings()
    {
        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        System.IO.File.WriteAllText(settingsPath, json);
    }
}