using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------

    [Header("Movement")]
    /// <summary>Movement speed while walking.</summary>
    public float walkSpeed = 10f;
    /// <summary>Movement speed while running (requires stamina).</summary>
    public float runSpeed = 16f;

    [Header("Stamina")]
    /// <summary>Maximum stamina value. Stamina depletes while running and recovers at rest.</summary>
    public float maxStamina = 100f;
    /// <summary>Rate at which stamina drains while running and recovers while not running (units/sec).</summary>
    public float staminaRate = 10f;
    /// <summary>Optional UI Slider to display stamina. Assign in the Inspector.</summary>
    public Slider staminaBar;

    [Header("Mouse Look")]
    /// <summary>Horizontal mouse sensitivity. Loaded from PlayerPrefs at start ("MouseSensitivity").</summary>
    public float camSensitivity = 5f;

    [Header("References")]
    /// <summary>The CharacterController used for movement. Auto-assigned if not set in the Inspector.</summary>
    public CharacterController cc;
    [SerializeField] GameObject jumpRopePrefab;
    GameObject jumpropeScreen;
    public Playtime whichBrat;

    [Space(15)]

    // -------------------------------------------------------------------------
    // Public State
    // -------------------------------------------------------------------------

    public bool inFaculty = false;

    public bool inOffice = false;

    public float guiltTime = 0f;

    public GuiltType guiltType;

    [Space(20)]

    public bool jumpRope = false;

    /// <summary>Set to false to freeze all player movement and rotation.</summary>
    public bool canMove = true;

    /// <summary>Current stamina value. Read by other scripts (e.g. CameraScript) to check run state.</summary>
    public float stamina;

    /// <summary>Set to true when the player has been caught by a Killer. Freezes movement and triggers game over logic.</summary>
    public bool gameOver = false;

    [SerializeField] private Material blackSky;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    /// <summary>The Y position the player is locked to. Set from the initial transform position.</summary>
    private float height;
    private float gameOverDelay = 0.5f;

    private float playerSpeed;
    private float targetYaw;        // Accumulated horizontal mouse input, applied directly each frame
    private Quaternion playerRotation;
    private Vector3 moveDirection, frozenPosition = Vector3.zero;
    private Camera playerCamera;

    private InputManager input;

    // -------------------------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------------------------

    private void Start()
    {
        input = Singleton<InputManager>.Instance;

        // Lock the player to their starting Y position (no gravity/vertical movement)
        height = transform.position.y;

        stamina = maxStamina;
        playerRotation = transform.rotation;
        targetYaw = transform.eulerAngles.y;

        camSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 4f);
        playerCamera = GetComponentInChildren<Camera>();

        if (cc == null)
            cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Keep the player at a fixed height — vertical movement is not supported
        if (canMove)
            transform.position = new Vector3(transform.position.x, height, transform.position.z);

        if (jumpRope && whichBrat != null && (transform.position - frozenPosition).magnitude >= 1f)
            ActivateJumpRope(false, whichBrat);

        MouseMove();
        PlayerMove();
        StaminaCheck();
        HandleGuilt();
    }

    private void LateUpdate()
    {
        if (gameOver)
        {
            Time.timeScale = 0f;
            RenderSettings.skybox = blackSky;
            canMove = false;
            gameOverDelay -= Time.unscaledDeltaTime * 0.5f;
            playerCamera.farClipPlane = gameOverDelay * 400f;

            if (gameOverDelay <= 0f)
            {
                SceneManager.LoadScene("MainMenu");
                Time.timeScale = 1f;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Input & Movement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads horizontal mouse input and applies it directly to the player's yaw rotation.
    /// </summary>
    private void MouseMove()
    {
        targetYaw += Input.GetAxis("Mouse X") * camSensitivity * Time.timeScale;
        playerRotation.eulerAngles = new Vector3(playerRotation.eulerAngles.x, targetYaw, 0f);
        transform.rotation = playerRotation;
    }

    /// <summary>
    /// Reads movement input, calculates move direction, and moves the CharacterController.
    /// Running is only allowed while stamina is above zero.
    /// </summary>
    private void PlayerMove()
    {
        Vector3 movement = Vector3.zero;
        Vector3 lateral = Vector3.zero;

        if (input.GetActionKey(InputAction.MoveForward)) movement = transform.forward;
        if (input.GetActionKey(InputAction.MoveBackward)) movement = -transform.forward;
        if (input.GetActionKey(InputAction.MoveLeft)) lateral = -transform.right;
        if (input.GetActionKey(InputAction.MoveRight)) lateral = transform.right;

        bool running = input.GetActionKey(InputAction.Run) && stamina > 0f;
        playerSpeed = running ? runSpeed : walkSpeed;

        moveDirection = (movement + lateral).normalized * playerSpeed * Time.deltaTime;

        if (canMove)
            cc.Move(moveDirection);
    }

    public bool IsMoving()
    {
        return canMove && moveDirection.sqrMagnitude > 0f;
    }

    private void StaminaCheck()
    {
        bool running = input.GetActionKey(InputAction.Run);

        if (cc.velocity.magnitude > 0.1f && running && stamina > 0f)
        {
            guiltTime = 0.1f;
            guiltType = GuiltType.Running;
            stamina -= staminaRate * Time.deltaTime;
            stamina = Mathf.Max(stamina, -5f);
        }
        else if (!running && stamina < maxStamina)
        {
            if (IsMoving()) return;
            stamina += staminaRate * Time.deltaTime;
        }

        if (staminaBar != null)
            staminaBar.value = stamina / maxStamina * 100f;
    }

    void HandleGuilt()
    {
        if (guiltTime > 0f)
            guiltTime -= Time.deltaTime;
    }

    public void ActivateJumpRope(bool state, Playtime brat, bool success = false)
    {
        if (state)
        {
            jumpRope = true;
            frozenPosition = transform.position;
            jumpropeScreen = Instantiate(jumpRopePrefab);
            whichBrat = brat;
        }
        else
        {
            jumpRope = false;
            Destroy(jumpropeScreen);
            if (!success)
                whichBrat.Disappoint();
            else
                whichBrat.Happy();
            whichBrat = null;
        }
    }
}