using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float voiceVolume = 1f;

    private List<ManagedAudioSource> registeredSources = new List<ManagedAudioSource>();
    private Queue<ManagedAudioSource> audioQueue = new Queue<ManagedAudioSource>();
    private bool isPlayingQueue = false;

    protected override void OnAwake()
    {
        LoadVolumeSettings();
    }

    // ─── Registration ─────────────────────────────────────────────
    public void Register(ManagedAudioSource source)
    {
        if (!registeredSources.Contains(source))
            registeredSources.Add(source);
    }

    public void Unregister(ManagedAudioSource source)
    {
        registeredSources.Remove(source);
    }

    // ─── Volume Setters ───────────────────────────────────────────
    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        ApplyVolumeToType(AudioType.Music);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
        ApplyVolumeToType(AudioType.SFX);
    }

    public void SetVoiceVolume(float value)
    {
        voiceVolume = value;
        PlayerPrefs.SetFloat("VoiceVolume", value);
        ApplyVolumeToType(AudioType.Voice);
    }

    void ApplyVolumeToType(AudioType type)
    {
        foreach (var source in registeredSources)
            if (source.audioType == type)
                source.ApplyVolume();
    }

    void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
    }

    // ─── Audio Queue ──────────────────────────────────────────────
    public void EnqueueAudio(ManagedAudioSource source)
    {
        audioQueue.Enqueue(source);
        if (!isPlayingQueue)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        isPlayingQueue = true;
        while (audioQueue.Count > 0)
        {
            ManagedAudioSource next = audioQueue.Dequeue();
            next.Play();
            yield return new WaitForSeconds(next.Source.clip.length);
        }
        isPlayingQueue = false;
    }

    public void ClearQueue()
    {
        audioQueue.Clear();
        isPlayingQueue = false;
    }
}