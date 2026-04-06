using UnityEngine;
using static Baldi;

public class KillerScript : MonoBehaviour
{
    public bool canKill = true;
    [SerializeField] float killHeight = 3f;
    [SerializeField] KillSound[] killSounds;
    [SerializeField] ManagedAudioSource sfxSource;

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
        if (sound.clip != null)
            source.clip = sound.clip;
        mas.Play(sound.subtitleKey);
    }
}