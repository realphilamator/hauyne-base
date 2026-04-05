using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class EditorLocalizationEntry
{
    public string key = "";
    public string value = "";
}

public class SubtitleLocalizationEditor : EditorWindow
{
    private List<EditorLocalizationEntry> entries = new List<EditorLocalizationEntry>();
    private Vector2 scrollPos;
    private string currentLanguage = "en";
    private List<string> availableLanguages = new List<string>();
    private string searchQuery = "";
    private bool isDirty = false;

    // New language dialog state
    private bool addingNewLanguage = false;
    private string newLanguageCode = "";

    [MenuItem("Tools/Subtitle Localization Editor")]
    public static void Open()
    {
        var window = GetWindow<SubtitleLocalizationEditor>("Subtitle Editor");
        window.minSize = new Vector2(500, 400);
        window.RefreshLanguageList();
        window.LoadFromFile();
    }

    private void OnGUI()
    {
        DrawLanguageBar();
        DrawToolbar();
        DrawSearchBar();
        DrawEntries();
        DrawAddButton();
        DrawFooter();

        if (addingNewLanguage)
            DrawNewLanguageDialog();
    }

    // ─── Language Bar ─────────────────────────────────────────────
    private void DrawLanguageBar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        foreach (string lang in availableLanguages)
        {
            bool isSelected = lang == currentLanguage;

            GUI.color = isSelected ? new Color(0.6f, 0.8f, 1f) : Color.white;

            if (GUILayout.Button(lang.ToUpper(), isSelected
                    ? EditorStyles.miniButtonMid
                    : EditorStyles.miniButton, GUILayout.Width(48), GUILayout.Height(22)))
            {
                if (!isSelected)
                    SwitchLanguage(lang);
            }
        }

        GUI.color = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("+ New Language", EditorStyles.miniButton, GUILayout.Height(22)))
        {
            addingNewLanguage = true;
            newLanguageCode = "";
        }

        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    // ─── New Language Dialog ──────────────────────────────────────
    private void DrawNewLanguageDialog()
    {
        // Darken background
        var overlay = new Rect(0, 0, position.width, position.height);
        EditorGUI.DrawRect(overlay, new Color(0, 0, 0, 0.4f));

        // Dialog box
        float w = 300f, h = 120f;
        var dialogRect = new Rect(
            (position.width - w) / 2f,
            (position.height - h) / 2f,
            w, h
        );

        GUILayout.BeginArea(dialogRect, EditorStyles.helpBox);

        EditorGUILayout.LabelField("New Language", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Language code (e.g. fr, de, ja):", EditorStyles.miniLabel);

        GUI.SetNextControlName("LangCodeField");
        newLanguageCode = EditorGUILayout.TextField(newLanguageCode).ToLower();
        EditorGUI.FocusTextInControl("LangCodeField");

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();

        bool isValid = !string.IsNullOrWhiteSpace(newLanguageCode)
                    && !availableLanguages.Contains(newLanguageCode)
                    && newLanguageCode.All(c => char.IsLetter(c));

        GUI.enabled = isValid;
        GUI.color = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("Create", GUILayout.Height(24)))
        {
            CreateNewLanguage(newLanguageCode);
            addingNewLanguage = false;
        }

        GUI.enabled = true;
        GUI.color = new Color(1f, 0.6f, 0.6f);
        if (GUILayout.Button("Cancel", GUILayout.Height(24)))
            addingNewLanguage = false;

        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        if (!isValid && !string.IsNullOrWhiteSpace(newLanguageCode))
        {
            if (availableLanguages.Contains(newLanguageCode))
                EditorGUILayout.LabelField("⚠ Language already exists.", EditorStyles.miniLabel);
            else
                EditorGUILayout.LabelField("⚠ Letters only (e.g. en, fr, de).", EditorStyles.miniLabel);
        }

        GUILayout.EndArea();
    }

    // ─── Toolbar ──────────────────────────────────────────────────
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button(isDirty ? "Save *" : "Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
            SaveToFile();

        if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            if (!isDirty || EditorUtility.DisplayDialog("Unsaved Changes", "Discard changes and reload from file?", "Yes", "No"))
                LoadFromFile();
        }

        GUILayout.FlexibleSpace();

        GUI.color = new Color(0.6f, 1f, 0.6f);
        GUILayout.Label($"{entries.Count} entries  |  subtitles_{currentLanguage}.json", EditorStyles.toolbarButton);
        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    // ─── Search Bar ───────────────────────────────────────────────
    private void DrawSearchBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField);
        if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)))
            searchQuery = "";
        EditorGUILayout.EndHorizontal();
    }

    // ─── Entry List ───────────────────────────────────────────────
    private void DrawEntries()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label("KEY", EditorStyles.boldLabel, GUILayout.Width(220));
        GUILayout.Label("VALUE", EditorStyles.boldLabel);
        GUILayout.Space(24);
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        string query = searchQuery.ToLower();
        int toRemove = -1;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (!string.IsNullOrEmpty(query) &&
                !entry.key.ToLower().Contains(query) &&
                !entry.value.ToLower().Contains(query))
                continue;

            if (i % 2 == 0)
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            else
                EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            string newKey = EditorGUILayout.TextField(entry.key, GUILayout.Width(220));
            string newVal = EditorGUILayout.TextField(entry.value);
            if (EditorGUI.EndChangeCheck())
            {
                entry.key = newKey;
                entry.value = newVal;
                isDirty = true;
            }

            GUI.color = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("✕", GUILayout.Width(22), GUILayout.Height(18)))
                toRemove = i;
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        if (toRemove >= 0)
        {
            entries.RemoveAt(toRemove);
            isDirty = true;
        }

        EditorGUILayout.EndScrollView();
    }

    // ─── Add Entry Button ─────────────────────────────────────────
    private void DrawAddButton()
    {
        EditorGUILayout.Space(4);
        GUI.color = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("+ Add Entry", GUILayout.Height(28)))
        {
            entries.Add(new EditorLocalizationEntry());
            isDirty = true;
        }
        GUI.color = Color.white;
    }

    // ─── Footer ───────────────────────────────────────────────────
    private void DrawFooter()
    {
        EditorGUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
        string path = GetFilePath(currentLanguage);
        GUI.color = new Color(0.7f, 0.7f, 0.7f);
        GUILayout.Label(File.Exists(path) ? $"📄 {path}" : $"⚠ File not found: {path}", EditorStyles.miniLabel);
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    // ─── Language Logic ───────────────────────────────────────────
    private void RefreshLanguageList()
    {
        availableLanguages.Clear();
        string dir = Application.streamingAssetsPath;

        if (!Directory.Exists(dir))
        {
            availableLanguages.Add("en");
            return;
        }

        foreach (string file in Directory.GetFiles(dir, "subtitles_*.json"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string lang = name.Replace("subtitles_", "");
            availableLanguages.Add(lang);
        }

        if (availableLanguages.Count == 0)
            availableLanguages.Add("en");
    }

    private void SwitchLanguage(string lang)
    {
        if (isDirty && !EditorUtility.DisplayDialog("Unsaved Changes",
            $"Save changes to subtitles_{currentLanguage}.json before switching?", "Save", "Discard"))
        {
            // Discard
        }
        else if (isDirty)
        {
            SaveToFile();
        }

        currentLanguage = lang;
        LoadFromFile();
    }

    private void CreateNewLanguage(string lang)
    {
        // Create an empty file for the new language
        string path = GetFilePath(lang);
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var file = new LocalizationFile { localizations = new List<LocalizationEntry>() };
        File.WriteAllText(path, JsonUtility.ToJson(file, true));

        AssetDatabase.Refresh();
        RefreshLanguageList();
        SwitchLanguage(lang);

        Debug.Log($"[SubtitleEditor] Created new language file: {path}");
    }

    // ─── Load / Save ──────────────────────────────────────────────
    private void LoadFromFile()
    {
        string path = GetFilePath(currentLanguage);
        entries = new List<EditorLocalizationEntry>();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SubtitleEditor] File not found: {path}");
            isDirty = false;
            return;
        }

        string json = File.ReadAllText(path);
        var file = JsonUtility.FromJson<LocalizationFile>(json);

        if (file == null || file.localizations == null)
        {
            Debug.LogWarning($"[SubtitleEditor] File is empty or invalid: {path}");
            isDirty = false;
            return;
        }

        foreach (var e in file.localizations)
            entries.Add(new EditorLocalizationEntry { key = e.key, value = e.value });

        isDirty = false;
        Debug.Log($"[SubtitleEditor] Loaded {entries.Count} entries from {path}");
    }

    private void SaveToFile()
    {
        string path = GetFilePath(currentLanguage);
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var file = new LocalizationFile { localizations = new List<LocalizationEntry>() };
        foreach (var e in entries)
            file.localizations.Add(new LocalizationEntry { key = e.key, value = e.value });

        File.WriteAllText(path, JsonUtility.ToJson(file, true));
        isDirty = false;
        AssetDatabase.Refresh();
        Debug.Log($"[SubtitleEditor] Saved {entries.Count} entries to {path}");
    }

    private string GetFilePath(string lang)
    {
        return Path.Combine(Application.streamingAssetsPath, $"subtitles_{lang}.json");
    }
}