using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baldi : NPC
{
    public enum BaldiState
    {
        Wandering,
        Targetting,
        Eating,
        Praise
    }

    [System.Serializable]
    public struct BaldiAudio
    {
        public AudioClip clip;
        public string subtitleKey;
        public float length;
    }

    public BaldiState currentState = BaldiState.Wandering;
    public bool antiHearing = false;
    bool wheredYouGo = false, canMove = true;
    float timeToMove = 0f, storedSpeed = 0f, moveFrames = 0f, tempAnger = 0f, anger = 0f, loseTimer = 0f, antiHearingTime = 30f, currentPriority = 0f;
    [SerializeField] float baseTime = 3f, baldiWait = 3f;
    [SerializeField] KillerScript killerScript;

    [Space(15)]

    [SerializeField] Animator baldiAnimator;
    [SerializeField] LipSync lipSyncAnim;

    [Space(15)]

    [SerializeField] List<Vector3> soundLocations = new List<Vector3>();

    [Space(15)]

    [SerializeField] BaldiAudio slapSound = new BaldiAudio { subtitleKey = "Sfx_Baldi_Slap", length = 0.7f };
    [SerializeField] BaldiAudio eatSound = new BaldiAudio { subtitleKey = "Sfx_AppleCrunch", length = 1f };
    [SerializeField] BaldiAudio crunchSound = new BaldiAudio { subtitleKey = "Sfx_Crunch", length = 0.6f };
    [SerializeField] BaldiAudio yumVoice = new BaldiAudio { subtitleKey = "Vfx_Baldi_Yum", length = 1f };
    [SerializeField] BaldiAudio appleVoice = new BaldiAudio { subtitleKey = "Vfx_Baldi_Apple", length = 3.5f };

    [SerializeField] List<BaldiAudio> praiseVoices = new List<BaldiAudio>();

    [Space(15)]

    [SerializeField] ManagedAudioSource sfxSource;
    [SerializeField] ManagedAudioSource vfxSource;
    AudioSource sfxAudio, vfxAudio;

    protected override void Awake()
    {
        base.Awake();
        antiHearing = false;

        timeToMove = baseTime;
        baldiWait = baseTime;

        storedSpeed = agent.speed;
        agent.speed = 0f;
        currentState = BaldiState.Wandering;

        sfxAudio = sfxSource.GetComponent<AudioSource>();
        vfxAudio = vfxSource.GetComponent<AudioSource>();
    }

    void Update()
    {
        coolDown = Mathf.Max(0f, coolDown - Time.deltaTime);

        if (timeToMove > 0f && canMove)
        {
            timeToMove -= Time.deltaTime;
            if (timeToMove <= 0f)
            {
                Move();
            }
        }

        if (antiHearingTime > 0f && antiHearing)
        {
            antiHearingTime -= Time.deltaTime;
            if (antiHearingTime <= 0f)
            {
                antiHearing = false;
                antiHearingTime = 30f;
            }
        }

        switch (currentState)
        {
            case BaldiState.Wandering:
                CheckHeardSources();
                if (soundLocations.Count == 0 && !agent.pathPending && agent.remainingDistance < 0.5f && coolDown <= 0f) // If the NPC is not currently moving towards a destination and the cooldown has expired, it gets a new location to move towards
                {
                    Vector3 newLocation = locationSelector.GetLocation(locations);
                    int attempts = 0;

                    while (Vector3.Distance(transform.position, newLocation) < 1f && attempts < 10) // If the new location is too close to the current position, it gets a new location
                    {
                        newLocation = locationSelector.GetLocation(locations);
                        attempts++;
                    }

                    agent.SetDestination(newLocation);
                    coolDown = 0.5f;
                }
                break;
            case BaldiState.Targetting:
                if (spottedPlayer)
                    agent.SetDestination(player.transform.position); // Move towards the player

                if (!agent.pathPending && agent.remainingDistance < 0.5f && coolDown <= 0f)
                {
                    if (!spottedPlayer) // In Plus, baldi doesn't lose the player easily unlike classic.
                    {
                        loseTimer = baldiWait > 1.8f ? 5f : 10f;
                        wheredYouGo = true;
                    }
                }

                if (loseTimer > 0f && wheredYouGo)
                {
                    loseTimer -= Time.deltaTime;
                    agent.SetDestination(player.transform.position);
                    if (loseTimer <= 0f)
                    {
                        wheredYouGo = false;
                        currentState = BaldiState.Wandering;
                    }
                }
                break;
            case BaldiState.Eating:
                if (agent.hasPath) agent.ResetPath();
                break;
            case BaldiState.Praise:
                if (agent.hasPath) agent.ResetPath();
                break;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (spottedPlayer && canMove)
            currentState = BaldiState.Targetting;

        if (moveFrames > 0f)
        {
            moveFrames -= 1f;
            agent.speed = storedSpeed;
            if (moveFrames <= 0f)
            {
                agent.speed = 0f;
                moveFrames = 0f;
            }
        }
    }

    void Move()
    {
        timeToMove = baldiWait - tempAnger;
        moveFrames = 10f;
        baldiAnimator.SetTrigger("Slap");
        PlaySound(sfxAudio, sfxSource, slapSound);
    }

    public void AddAnger(float value)
    {
        anger += value;
        if (anger < 0.5f)
        {
            anger = 0.5f;
        }
        baldiWait = -3 * anger / (anger + 2 / 0.65f) + 3f;
    }

    public void Hear(Vector3 source, float priority)
    {
        if (antiHearing)
            return;

        if (spottedPlayer)
            return;

        soundLocations.Add(source); // Save the sound location to go to later if it's not the highest priority sound

        if (priority > currentPriority)
        {
            agent.SetDestination(source);
            currentPriority = priority;
        }
    }

    public void AntiHearing()
    {
        antiHearing = true;
        ClearAllSoundsHeard();
    }

    public void ClearAllSoundsHeard() // Used for Anti Hearing and Principal's Office
    {
        soundLocations.Clear();
        currentPriority = 0f;
        if (agent.hasPath)
            agent.ResetPath();
    }

    void CheckHeardSources()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && soundLocations.Count > 0)
        {
            agent.SetDestination(soundLocations[0]);
            soundLocations.RemoveAt(0);
            currentPriority = 0f;
        }
    }

    [ContextMenu("Eat Apple")]
    void AppleMuncher()
    {
        killerScript.canKill = false;
        currentState = BaldiState.Eating;
        canMove = false;
        baldiAnimator.SetTrigger("EatIdle");
        PlaySound(vfxAudio, vfxSource, appleVoice);
        StartCoroutine(appleMunching());
    }

    IEnumerator appleMunching()
    {
        yield return new WaitForSeconds(appleVoice.clip.length + 1f);
        float timer = 0f;
        float duration = 10f;

        float interval = 0.05f;
        float yumInterval = 2f;
        float yumTimer = 0f;

        baldiAnimator.SetBool("Eating", true);

        while (timer < duration)
        {
            yumTimer += interval;

            if (yumTimer >= yumInterval)
            {
                vfxSource.PlayOneShot(yumVoice.clip, yumVoice.subtitleKey, yumVoice.length);
                yumTimer = 0f;
            }

            BaldiAudio selectedAudio = Random.Range(0, 2) == 0 ? eatSound : crunchSound;
            sfxSource.PlayOneShot(selectedAudio.clip, selectedAudio.subtitleKey, selectedAudio.length);

            yield return new WaitForSeconds(interval);

            timer += interval;
            yield return null;
        }

        canMove = true;
        baldiAnimator.SetBool("Eating", false);
        currentState = BaldiState.Wandering;
        killerScript.canKill = true;
        yield break;
    }

    [ContextMenu("Praise")]
    public void Praise()
    {
        BaldiAudio selectedPraise = praiseVoices[Random.Range(0, praiseVoices.Count)];
        canMove = false;
        currentState = BaldiState.Praise;
        killerScript.canKill = false;
        PlaySound(vfxAudio, vfxSource, selectedPraise);
        StartCoroutine(PraiseCoroutine(selectedPraise.clip.length));
    }

    IEnumerator PraiseCoroutine(float duration)
    {
        baldiAnimator.enabled = false;
        lipSyncAnim.enabled = true;
        yield return new WaitForSeconds(duration + 3f);
        lipSyncAnim.enabled = false;
        baldiAnimator.enabled = true;
        currentState = BaldiState.Wandering;
        canMove = true;
        killerScript.canKill = true;
        yield break;
    }

    void PlaySound(AudioSource source, ManagedAudioSource mas, BaldiAudio sound)
    {
        if (source == null || mas == null)
            return;
        if (sound.clip != null)
            source.clip = sound.clip;
        mas.Play(sound.subtitleKey, sound.length);
    }
}
