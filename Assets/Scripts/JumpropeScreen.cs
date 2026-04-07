using TMPro;
using UnityEngine;

public class JumpropeScreen : MonoBehaviour
{
    PlayerController player;
    CameraScript cs;
    [SerializeField] int requiredJumps = 5;
    float jumpDelay = 0f, ropePosition = 0f;
    bool ropeHit = true, jumpStarted = false;
    int jumps = 0;
    [SerializeField] TMP_Text jumpCount, instructions;
    [SerializeField] Animator ropeAnimator;
    Playtime playtime;

    private void OnEnable()
    {
        player = FindObjectOfType<PlayerController>();
        cs = player.GetComponentInChildren<CameraScript>();
        playtime = player.whichBrat;
        jumpDelay = 1f;
        ropeHit = true;
        jumpStarted = false;
        jumps = 0;
        jumpCount.text = $"{jumps}/{requiredJumps}";
        instructions.text = $"Time to jump rope!\nPress {Singleton<InputManager>.Instance.GetActionKey(InputAction.Jump)} to jump!";
        cs.jumpHeight = 0f;
        playtime.PlayAudio(playtime.readyGo);
    }

    private void Update()
    {
        if (jumpDelay > 0f)
        {
            jumpDelay -= Time.deltaTime;
            if (jumpDelay <= 0f && !jumpStarted)
            {
                jumpStarted = true;
                ropePosition = 1f;
                ropeAnimator.SetTrigger("Jumprope");
                ropeHit = false;
            }
        }

        if (ropePosition > 0f)
        {
            ropePosition -= Time.deltaTime;
            if (ropePosition <= 0f && !ropeHit)
                RopeHit();
        }
    }

    void RopeHit()
    {
        ropeHit = true;
        if (cs.jumpHeight <= 0.2f)
            Fail();
        else
            Success();
        jumpStarted = false;
    }

    void Success()
    {
        playtime.VoiceCount(jumps);
        jumps++;
        jumpCount.text = $"{jumps}/{requiredJumps}";
        jumpDelay = 0.5f;
        if (jumps >= 5)
            player.ActivateJumpRope(false, playtime, true);
    }

    void Fail()
    {
        jumps = 0;
        jumpCount.text = $"{jumps}/{requiredJumps}";
        jumpDelay = 2f;
        playtime.PlayAudio(playtime.messedUp);
    }
}