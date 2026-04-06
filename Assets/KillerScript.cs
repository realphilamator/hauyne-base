using UnityEngine;
using static Baldi;

public class KillerScript : MonoBehaviour
{
    public bool canKill = true;
    public float killHeight = 3f;
    [SerializeField] KillSound[] killSounds;
    [SerializeField] ManagedAudioSource sfxSource;

    [System.Serializable]
    public struct KillSound
    {
        public AudioClip clip;
        public string subtitleKey;
    }

    public void PlayKillSound()
    {
        if (killSounds.Length == 0) return;
        KillSound sound = killSounds[Random.Range(0, killSounds.Length)];
        PlaySound(sfxSource.GetComponent<AudioSource>(), sfxSource, sound);
    }

    void PlaySound(AudioSource source, ManagedAudioSource mas, KillSound sound)
    {
        if (source == null || mas == null)
            return;

        mas.SetClipAndPlay(sound.clip, sound.subtitleKey, sound.clip.length);
    }
}