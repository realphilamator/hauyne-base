using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}

[System.Serializable]
public class LocalizationFile
{
    public List<LocalizationEntry> localizations;
}

public class LocalizationManager : Singleton<LocalizationManager>
{
    private Dictionary<string, string> table = new Dictionary<string, string>();

    protected override void OnAwake()
    {
        LoadJSON("subtitles_en");
    }

    public void LoadJSON(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName + ".json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Localization file not found: {path}");
            return;
        }
        string json = File.ReadAllText(path);
        var file = JsonUtility.FromJson<LocalizationFile>(json);
        table.Clear();
        foreach (var entry in file.localizations)
            table[entry.key] = entry.value;

        Debug.Log($"Loaded {table.Count} localization entries.");
    }

    public string Get(string key)
    {
        if (table.TryGetValue(key, out string val))
            return val;
        Debug.LogWarning($"Missing localization key: {key}");
        return $"[{key}]";
    }
}