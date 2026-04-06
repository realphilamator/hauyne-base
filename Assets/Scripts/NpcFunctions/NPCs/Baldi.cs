using UnityEngine;

public class Baldi : NPC
{
    public enum BaldiState
    {
        Wandering,
        Targetting,
        Eating
    }

    [System.Serializable]
    public struct BaldiAudio
    {
        public AudioClip clip;
        public string subtitleKey;
    }

    public BaldiState currentState = BaldiState.Wandering;
    public bool antiHearing = false;
    bool wheredYouGo = false;
    float timeToMove = 0f, storedSpeed = 0f, moveFrames = 0f, tempAnger = 0f, anger = 0f, loseTimer = 0f;
    [SerializeField] float baseTime = 3f, baldiWait = 3f;

    [Space(15)]

    [SerializeField] Animator baldiAnimator;

    [Space(15)]

    [SerializeField] BaldiAudio slapSound = new BaldiAudio { subtitleKey = "Sfx_Baldi_Slap" };
    [SerializeField] BaldiAudio eatSound = new BaldiAudio { subtitleKey = "Sfx_Crunch" };
    [SerializeField] BaldiAudio yumVoice = new BaldiAudio { subtitleKey = "Vfx_Baldi_Yum" };
    [SerializeField] BaldiAudio appleVoice = new BaldiAudio { subtitleKey = "Vfx_Baldi_Apple" };

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

        if (timeToMove > 0f)
        {
            timeToMove -= Time.deltaTime;
            if (timeToMove <= 0f)
            {
                Move();
            }
        }

        switch (currentState)
        {
            case BaldiState.Wandering:
                if (!agent.pathPending && agent.remainingDistance < 0.5f && coolDown <= 0f) // If the NPC is not currently moving towards a destination and the cooldown has expired, it gets a new location to move towards
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
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (spottedPlayer)
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

    void PlaySound(AudioSource source, ManagedAudioSource mas, BaldiAudio sound)
    {
        if (source == null || mas == null)
            return;
        if (sound.clip != null)
            source.clip = sound.clip;
        mas.Play(sound.subtitleKey);
    }
}
