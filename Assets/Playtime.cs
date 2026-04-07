using UnityEngine;
using static Baldi;

public class Playtime : NPC
{
    public enum PlaytimeState
    {
        Wandering,
        Targetting,
        Playing
    }

    public PlaytimeState currentState;

    float playingCooldown = 0f;
    bool startedJumprope = false, I_spottedPlayer = false;

    [SerializeField] Animator playAnimator;
    [Space(15)]
    [SerializeField] PlaytimeAudio laugh;
    [SerializeField] PlaytimeAudio wannaPlay;
    [SerializeField] PlaytimeAudio spottedAudio;
    public PlaytimeAudio readyGo;
    public PlaytimeAudio messedUp;
    [SerializeField] PlaytimeAudio congrats;
    [SerializeField] PlaytimeAudio sadAudio;
    [SerializeField] PlaytimeAudio[] countAudio;
    [Space(5)]
    [SerializeField] ManagedAudioSource musicSource;
    [SerializeField] ManagedAudioSource voiceSource;

    [System.Serializable]
    public struct PlaytimeAudio
    {
        public AudioClip clip;
        public string subtitleKey;
        public float length;
    }

    protected override void Awake()
    {
        base.Awake();
        currentState = PlaytimeState.Wandering;
    }

    private void Update()
    {
        coolDown = Mathf.Max(0, coolDown - Time.deltaTime);

        if (playingCooldown > 0f)
        {
            playingCooldown -= Time.deltaTime;
            if (playingCooldown <= 0f)
            {
                if (playAnimator.GetBool("disappointed"))
                {
                    playingCooldown = 0f;
                    playAnimator.SetBool("disappointed", false);
                }
            }
        }

        switch (currentState)
        {
            case PlaytimeState.Wandering:
                I_spottedPlayer = false;
                if (!agent.pathPending && agent.remainingDistance < 0.5f && coolDown <= 0f) // If the NPC is not currently moving towards a destination and the cooldown has expired, it gets a new location to move towards
                {
                    Vector3 newLocation = locationSelector.GetLocation(locations);
                    int attempts = 0;

                    while (Vector3.Distance(transform.position, newLocation) < 1f && attempts < 10) // If the new location is too close to the current position, it gets a new location
                    {
                        newLocation = locationSelector.GetLocation(locations);
                        attempts++;
                    }

                    int audioIndex = Random.Range(0, 2);
                    PlaytimeAudio chosenAudio = audioIndex == 0 ? laugh : wannaPlay;
                    if (!voiceSource.Source.isPlaying)
                        voiceSource.SetClipAndPlay(chosenAudio.clip, chosenAudio.subtitleKey, chosenAudio.length);

                    agent.SetDestination(newLocation);
                    agent.speed = 15f;
                    coolDown = 0.5f;
                }
                break;
            case PlaytimeState.Targetting:
                TargetPlayer();
                break;
            case PlaytimeState.Playing:
                break;
        }
    }

    protected override void FixedUpdate()
    {
        if (!player.jumpRope)
        {
            base.FixedUpdate();

            if (spottedPlayer)
                currentState = PlaytimeState.Targetting;
            else if (spottedPlayer)
                currentState = PlaytimeState.Wandering;
            else if (agent.velocity.magnitude <= 1f && coolDown <= 0f)
                currentState = PlaytimeState.Wandering;

            startedJumprope = false;
        }
        else
        {
            currentState = PlaytimeState.Playing;
            if (!startedJumprope)
            {
                if (agent.hasPath)
                    agent.ResetPath();
                agent.SetDestination(transform.position - transform.forward * 10f);
            }
            startedJumprope = true;
            playingCooldown = 15f;
        }
    }

    protected override void TargetPlayer()
    {
        base.TargetPlayer();
        playAnimator.SetBool("disappointed", false);
        agent.speed = 20f;
        if (!I_spottedPlayer)
        {
            spottedPlayer = true;
            voiceSource.SetClipAndPlay(spottedAudio.clip, spottedAudio.subtitleKey, spottedAudio.length);
        }
    }

    public void Disappoint()
    {
        playAnimator.SetBool("disappointed", true);
        voiceSource.Stop();
        voiceSource.SetClipAndPlay(messedUp.clip, messedUp.subtitleKey, messedUp.length);
    }

    public void VoiceCount(int count)
    {
        if (count > 0 && count <= countAudio.Length)
        {
            PlaytimeAudio audio = countAudio[count - 1];
            voiceSource.SetClipAndPlay(audio.clip, audio.subtitleKey, audio.length);
        }
    }

    public void PlayAudio(PlaytimeAudio audio)
    {
        voiceSource.SetClipAndPlay(audio.clip, audio.subtitleKey, audio.length);
    }

    public void Happy()
    {
        playAnimator.SetBool("disappointed", false);
        voiceSource.Stop();
        voiceSource.SetClipAndPlay(congrats.clip, congrats.subtitleKey, congrats.length);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            if (!player.jumpRope && playingCooldown <= 0f)
                player.ActivateJumpRope(true, this);
        }
    }
}
