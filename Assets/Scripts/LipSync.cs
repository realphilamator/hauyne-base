using UnityEngine;
using UnityEngine.UI;

public class LipSync : MonoBehaviour // FROM SIMPLIFIED BUT MODIFIED SLIGHTLY I THINK, BE SURE TO CREDIT
{
    #region Event Functions
    private void Start()
    {
        if (useSprites)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            image = GetComponent<Image>();
        }
        else
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (!audioDevice.isPlaying) return;

        float volumeLevel = GetVolumeLevel();

        if (useSprites)
        {
            int currentFrame = volumeLevel >= volumeThreshold ? Mathf.FloorToInt(volumeLevel * (syncedSprites.Length - 1)) : 0;

            Sprite[] spritesToUse = altSprites ? syncedAltSprites : syncedSprites;

            if (spriteRenderer != null) spriteRenderer.sprite = spritesToUse[currentFrame];
            if (image != null) image.sprite = spritesToUse[currentFrame];
        }
        else
        {
            float normalizedTime = volumeLevel < volumeThreshold ? 1 - volumeLevel : volumeLevel;
            animator.Play(animName, -1, normalizedTime);
        }
    }
    #endregion

    #region Audio Volume Analysis
    private float GetVolumeLevel()
    {
        audioDevice.GetOutputData(samples, 0);
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += Mathf.Abs(samples[i]);
        }
        float average = sum / samples.Length;
        return Mathf.Clamp01(average / volumeThreshold);
    }
    #endregion

    #region Configuration
    [Header("Sync References And Settings")]
    [SerializeField] private AudioSource audioDevice;
    [SerializeField] private Sprite[] syncedSprites, syncedAltSprites;
    [SerializeField] private float volumeThreshold = 0.1f;
    [SerializeField] private string animName;
    [SerializeField] private bool useSprites, altSprites;
    #endregion

    #region Internal State
    private SpriteRenderer spriteRenderer;
    private Image image;
    private Animator animator;
    private float[] samples = new float[256];
    #endregion
}