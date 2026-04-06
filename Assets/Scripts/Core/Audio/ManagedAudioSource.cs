using UnityEngine;
using NaughtyAttributes;
[RequireComponent(typeof(AudioSource))]
public class ManagedAudioSource : MonoBehaviour
{
    [Header("Audio Type")]
    public AudioType audioType = AudioType.SFX;
    [Header("Subtitle Settings")]
    [ShowIf("HasSubtitleTypes")]
    public string subtitleKey;
    [ShowIf("HasSubtitleTypes")]
    public float subDuration = 2f;
    [ShowIf("HasSubtitleTypes")]
    public Color subtitleColor = Color.white;
    [ShowIf("HasSubtitleTypes")]
    public bool positional = true;
    private AudioSource audioSource;
    private bool HasSubtitleTypes => audioType == AudioType.SFX || audioType == AudioType.Voice;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AudioManager.Instance.Register(this);
        ApplyVolume();
    }
    void OnDestroy()
    {
        if (AudioManager.Exist)
            AudioManager.Instance.Unregister(this);
    }
    public void ApplyVolume()
    {
        audioSource.volume = audioType switch
        {
            AudioType.Music => AudioManager.Instance.musicVolume,
            AudioType.SFX => AudioManager.Instance.sfxVolume,
            AudioType.Voice => AudioManager.Instance.voiceVolume,
            _ => 1f
        };
    }
    public void Play(string subtitleKeyOverride = null, float durationOverride = -1f)
    {
        audioSource.Play();
        string keyToUse = subtitleKeyOverride ?? subtitleKey;
        if (!string.IsNullOrEmpty(keyToUse) && SubtitleManager.Instance != null)
            SubtitleManager.Instance.SpawnSubtitle(this, transform, keyToUse, durationOverride);
    }
    public void SetClipAndPlay(AudioClip clip, string subtitleKeyOverride = null, float durationOverride = -1f)
    {
        audioSource.clip = clip;
        Play(subtitleKeyOverride, durationOverride);
    }
    public float GetSubtitleScale(Transform cameraTransform)
    {
        return Mathf.Max(1f - Vector3.Distance(cameraTransform.position, transform.position) / audioSource.maxDistance, 0f);
    }
    public void Stop() => audioSource.Stop();
    public void Pause() => audioSource.Pause();
    public void UnPause() => audioSource.UnPause();
    public AudioSource Source => audioSource;
}