using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { Music, SFX, Voice }
    public VolumeType volumeType;

    void Start()
    {
        Slider slider = GetComponent<Slider>();
        slider.value = volumeType switch
        {
            VolumeType.Music => AudioManager.Instance.musicVolume,
            VolumeType.SFX => AudioManager.Instance.sfxVolume,
            VolumeType.Voice => AudioManager.Instance.voiceVolume,
            _ => 1f
        };

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        switch (volumeType)
        {
            case VolumeType.Music: AudioManager.Instance.SetMusicVolume(value); break;
            case VolumeType.SFX: AudioManager.Instance.SetSFXVolume(value); break;
            case VolumeType.Voice: AudioManager.Instance.SetVoiceVolume(value); break;
        }
    }
}
