using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [System.Serializable]
    public class SettingsMenu
    {
        [TextArea(1, 3)]
        public string menuName;
        public GameObject panel;
    }

    public SettingsMenu[] menus;
    private int currentIndex = 0;

    public ManagedAudioSource audioSource;

    [Header("Header UI")]
    public TextMeshProUGUI currentMenuLabel;
    public TextMeshProUGUI prevMenuLabel;
    public TextMeshProUGUI nextMenuLabel;

    [Header("Arrow Buttons")]
    public StandardButton prevButton;
    public StandardButton nextButton;

    [Header("Miscellaneous Toggles")]
    public ToggleControl captionsToggle;

    [Header("Settings Bars")]
    public SegmentedBar musicBar;
    public SegmentedBar sfxBar;
    public SegmentedBar voiceBar;
    public SegmentedBar mouseBar;

    [Header("Graphics UI")]
    public TextMeshProUGUI resolutionLabel;
    public StandardButton resPrevButton;
    public StandardButton resNextButton;
    public ToggleControl fullscreenToggle;
    public ToggleControl vsyncToggle;
    public ToggleControl pixelFilterToggle;
    public StandardButton applyButton;

    // ─── PlayerPrefs Keys ─────────────────────────────────────────────────────
    private const string k_Music = "MusicVolume";
    private const string k_SFX = "SFXVolume";
    private const string k_Voice = "VoiceVolume";
    private const string k_Mouse = "MouseSensitivity";
    private const string k_Captions = "Captions";
    private const string k_Fullscreen = "Fullscreen";
    private const string k_Vsync = "Vsync";
    private const string k_PixelFilter = "PixelFilter";

    // ─── Resolution ───────────────────────────────────────────────────────────
    private List<Resolution> availableResolutions = new List<Resolution>();
    private int resIndex = 0;

    private void BuildResolutionList()
    {
        availableResolutions.Clear();

        Resolution min = new Resolution();
        min.width = 480; min.height = 360;
        availableResolutions.Add(min);

        foreach (Resolution r in Screen.resolutions)
        {
            bool exists = false;
            foreach (Resolution existing in availableResolutions)
            {
                if (existing.width == r.width && existing.height == r.height)
                { exists = true; break; }
            }
            if (!exists) availableResolutions.Add(r);
        }

        Resolution native = new Resolution();
        native.width = Display.main.systemWidth;
        native.height = Display.main.systemHeight;
        bool nativeExists = false;
        foreach (Resolution r in availableResolutions)
        {
            if (r.width == native.width && r.height == native.height)
            { nativeExists = true; break; }
        }
        if (!nativeExists) availableResolutions.Add(native);

        int savedW = PlayerPrefs.GetInt("ResW", Display.main.systemWidth);
        int savedH = PlayerPrefs.GetInt("ResH", Display.main.systemHeight);
        bool savedExists = false;
        foreach (Resolution r in availableResolutions)
        {
            if (r.width == savedW && r.height == savedH)
            { savedExists = true; break; }
        }
        if (!savedExists)
        {
            Resolution saved = new Resolution();
            saved.width = savedW;
            saved.height = savedH;
            availableResolutions.Add(saved);
        }

        for (int i = availableResolutions.Count - 1; i >= 0; i--)
        {
            int w = availableResolutions[i].width;
            int h = availableResolutions[i].height;

            if (w < 480 || h < 360) { availableResolutions.RemoveAt(i); continue; }

            float aspect = (float)w / h;
            if (aspect < 1.3f || aspect > 2.4f) availableResolutions.RemoveAt(i);
        }

        availableResolutions.Sort((a, b) =>
        {
            if (a.width != b.width) return a.width.CompareTo(b.width);
            return a.height.CompareTo(b.height);
        });

        resIndex = 0;
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == savedW && availableResolutions[i].height == savedH)
            { resIndex = i; break; }
        }
    }

    // ─── Start ────────────────────────────────────────────────────────────────
    private void Start()
    {
        prevButton.OnPress.AddListener(OnPrevPressed);
        nextButton.OnPress.AddListener(OnNextPressed);

        resPrevButton.OnPress.AddListener(OnResPrev);
        resNextButton.OnPress.AddListener(OnResNext);
        applyButton.OnPress.AddListener(ApplyGraphics);

        BuildResolutionList();
        ShowMenu(currentIndex);
        LoadPlayerPrefs();
    }

    // ─── Load ─────────────────────────────────────────────────────────────────
    private void LoadPlayerPrefs()
    {
        if (musicBar != null)
        {
            float saved = PlayerPrefs.GetFloat(k_Music, AudioManager.Instance.musicVolume);
            musicBar.Value = saved;
            AudioManager.Instance.SetMusicVolume(saved);
        }
        if (sfxBar != null)
        {
            float saved = PlayerPrefs.GetFloat(k_SFX, AudioManager.Instance.sfxVolume);
            sfxBar.Value = saved;
            AudioManager.Instance.SetSFXVolume(saved);
        }
        if (voiceBar != null)
        {
            float saved = PlayerPrefs.GetFloat(k_Voice, AudioManager.Instance.voiceVolume);
            voiceBar.Value = saved;
            AudioManager.Instance.SetVoiceVolume(saved);
        }
        if (mouseBar != null)
        {
            float saved = PlayerPrefs.GetFloat(k_Mouse, 3f);
            mouseBar.Value = saved;
        }

        captionsToggle?.SetSilent(PlayerPrefs.GetInt(k_Captions, 0) == 1);
        fullscreenToggle?.SetSilent(PlayerPrefs.GetInt(k_Fullscreen, 1) == 1);
        vsyncToggle?.SetSilent(PlayerPrefs.GetInt(k_Vsync, 1) == 1);
        pixelFilterToggle?.SetSilent(PlayerPrefs.GetInt(k_PixelFilter, 1) == 1);

        UpdateResolutionLabel();
    }

    // ─── Menu Navigation ──────────────────────────────────────────────────────
    public void OnNextPressed()
    {
        currentIndex = (currentIndex + 1) % menus.Length;
        ShowMenu(currentIndex);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPrevPressed()
    {
        currentIndex = (currentIndex - 1 + menus.Length) % menus.Length;
        ShowMenu(currentIndex);
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void ShowMenu(int index)
    {
        foreach (var menu in menus)
            menu.panel.SetActive(false);

        menus[index].panel.SetActive(true);
        currentMenuLabel.text = menus[index].menuName.Replace("\\n", "\n");

        int prevIndex = (index - 1 + menus.Length) % menus.Length;
        int nextIndex = (index + 1) % menus.Length;
        prevMenuLabel.text = menus[prevIndex].menuName.Replace("\\n", "\n");
        nextMenuLabel.text = menus[nextIndex].menuName.Replace("\\n", "\n");
    }

    // ─── Audio ────────────────────────────────────────────────────────────────
    public void SetMusicVolume(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat(k_Music, value);
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat(k_SFX, value);
    }

    public void SetVoiceVolume(float value)
    {
        AudioManager.Instance.SetVoiceVolume(value);
        PlayerPrefs.SetFloat(k_Voice, value);
    }

    // ─── General ──────────────────────────────────────────────────────────────
    public void SetMouseSensitivity(float value) => PlayerPrefs.SetFloat(k_Mouse, value);

    public void SetCaptions(bool value) => PlayerPrefs.SetInt(k_Captions, value ? 1 : 0);

    // ─── Graphics ─────────────────────────────────────────────────────────────
    public void OnResPrev()
    {
        resIndex = (resIndex - 1 + availableResolutions.Count) % availableResolutions.Count;
        UpdateResolutionLabel();
    }

    public void OnResNext()
    {
        resIndex = (resIndex + 1) % availableResolutions.Count;
        UpdateResolutionLabel();
    }

    private void UpdateResolutionLabel()
    {
        if (resolutionLabel == null) return;
        Resolution r = availableResolutions[resIndex];
        resolutionLabel.text = $"{r.width}x{r.height}";
    }

    public void ApplyGraphics()
    {
        bool fs = fullscreenToggle != null && fullscreenToggle.Value;
        bool vsync = vsyncToggle != null && vsyncToggle.Value;
        bool pixel = pixelFilterToggle != null && pixelFilterToggle.Value;

        Resolution r = availableResolutions[resIndex];
        Screen.SetResolution(r.width, r.height, fs);

        QualitySettings.vSyncCount = vsync ? 1 : 0;

        QualitySettings.masterTextureLimit = 0;
        foreach (Texture t in Resources.FindObjectsOfTypeAll<Texture>())
            t.filterMode = pixel ? FilterMode.Point : FilterMode.Bilinear;

        PlayerPrefs.SetInt("ResW", r.width);
        PlayerPrefs.SetInt("ResH", r.height);
        PlayerPrefs.SetInt(k_Fullscreen, fs ? 1 : 0);
        PlayerPrefs.SetInt(k_Vsync, vsync ? 1 : 0);
        PlayerPrefs.SetInt(k_PixelFilter, pixel ? 1 : 0);
        PlayerPrefs.Save();
    }
}