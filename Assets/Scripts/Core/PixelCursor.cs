using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PixelCursor : Singleton<PixelCursor>
{
    [Header("Cursor Sprite")]
    public Image cursorImage;
    public Sprite cursorSprite;

    [Header("Size & Hot-spot")]
    public float cursorSizePx = 16f;
    public Vector2 hotspot = Vector2.zero;

    [Header("Sensitivity")]
    public float controllerSensitivity = 200f;

    [Header("Reference Resolution")]
    public float referenceWidth = 480f;
    public float referenceHeight = 360f;

    [Header("Bounds (canvas-space). Leave at zero to auto-fill from reference res.)")]
    public Vector2 minRange = Vector2.zero;
    public Vector2 maxRange = Vector2.zero;

    [Header("Button Interaction")]
    public GraphicRaycaster graphicRaycaster;
    public string clickInputName = "Submit";

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip audHighlight;
    public AudioClip audConfirm;

    // When false: pixel cursor is hidden, system mouse cursor is freed.
    // When true:  pixel cursor is active, system mouse cursor is hidden.
    [Header("Mouse Mode")]
    [Tooltip("Toggle between pixel-cursor mode and free system-mouse mode.")]
    public bool pixelCursorActive = true;

    public Vector2 Position => _position;

    private RectTransform _rt;
    private Canvas _canvas;
    private RectTransform _canvasRect;
    private EventSystem _eventSystem;
    private PointerEventData _pointerEventData;
    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private Vector2 _position;
    private Vector3 _localPosition;
    private bool _hidden;
    private int _blinkFrames;
    private float _speedMultiplier = 1f;
    private StandardButton _heldButton;
    private StandardButton _lastHighlighted;

    // Raw mouse delta this frame (screen pixels)
    private Vector2 _mouseDelta;
    // Controller analog input this frame
    private Vector2 _analogInput;
    // Combined movement applied this frame
    private Vector2 _movementThisFrame;

    protected override void OnAwake()
    {
        _canvas = GetComponent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();
        _rt = cursorImage.GetComponent<RectTransform>();

        _rt.pivot = new Vector2(0f, 1f);
        _rt.anchorMin = new Vector2(0.5f, 0.5f);
        _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.sizeDelta = new Vector2(cursorSizePx, cursorSizePx);

        if (cursorSprite != null)
            cursorImage.sprite = cursorSprite;

        cursorImage.raycastTarget = false;

        _eventSystem = FindObjectOfType<EventSystem>();

        if (minRange == Vector2.zero && maxRange == Vector2.zero)
        {
            minRange = new Vector2(-referenceWidth / 2f, -referenceHeight / 2f);
            maxRange = new Vector2(referenceWidth / 2f, referenceHeight / 2f);
        }

        _position = Vector2.zero;
        ApplyPosition();

        ApplyMouseMode();

        Debug.Log($"[PixelCursor] Awake | canvas={_canvas.name}");
    }

    private void Update()
    {
        // --- Mouse mode toggle (example: press M to switch; wire however you like) ---
        // Remove or replace this block if you drive pixelCursorActive from elsewhere.
        // if (Input.GetKeyDown(KeyCode.M))
        //     SetPixelCursorActive(!pixelCursorActive);

        if (!pixelCursorActive)
        {
            // Free-mouse mode: hide pixel cursor, restore system cursor.
            if (cursorImage.enabled) cursorImage.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            ClearHeldButton();
            return;
        }

        // --- Blink timer ---
        if (_blinkFrames > 0)
        {
            _blinkFrames--;
            if (_blinkFrames <= 0)
                SetHidden(false);
        }

        // --- Cursor lock guard ---
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (cursorImage.enabled) cursorImage.enabled = false;
            return;
        }

        if (!_hidden && !cursorImage.enabled) cursorImage.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        // --- Speed boost (controller) ---
        // Wire to your InputManager if available; plain keyboard fallback shown here.
        _speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? 4f : 1f;

        // --- Delta-based movement (does NOT copy mouse position) ---
        _mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        _analogInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float mouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat("MouseSensitivity", 3f), 0.1f, 10f);

        _movementThisFrame.x = _mouseDelta.x * mouseSensitivity
                             + _analogInput.x * Time.unscaledDeltaTime * controllerSensitivity * _speedMultiplier;
        _movementThisFrame.y = _mouseDelta.y * mouseSensitivity
                             + _analogInput.y * Time.unscaledDeltaTime * controllerSensitivity * _speedMultiplier;

        _position.x = Mathf.Clamp(_position.x + _movementThisFrame.x, minRange.x, maxRange.x);
        _position.y = Mathf.Clamp(_position.y + _movementThisFrame.y, minRange.y, maxRange.y);

        ApplyPosition();

        // --- Button interaction ---
        if (_hidden) { ClearHeldButton(); return; }
        if (graphicRaycaster == null || _eventSystem == null) { ClearHeldButton(); return; }

        // Build a pointer event from the hotspot world position so raycasting
        // originates from the tip of the cursor, not the sprite corner.
        // _position is in canvas-local space; convert to world then to screen.
        Vector3 hotspotWorld = _canvasRect.TransformPoint(new Vector3(_position.x, _position.y, 0f));
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            hotspotWorld
        );

        _pointerEventData = new PointerEventData(_eventSystem);
        _pointerEventData.position = screenPos;

        _raycastResults.Clear();
        graphicRaycaster.Raycast(_pointerEventData, _raycastResults);

        StandardButton hoveredButton = null;
        foreach (var result in _raycastResults)
        {
            var btn = result.gameObject.GetComponentInParent<StandardButton>();
            if (btn != null) { hoveredButton = btn; break; }
        }

        if (hoveredButton != _lastHighlighted)
        {
            Debug.Log(hoveredButton != null
                ? $"[PixelCursor] Hovering: {hoveredButton.gameObject.name}"
                : "[PixelCursor] No button hovered");
        }

        if (hoveredButton != null)
        {
            if (hoveredButton != _lastHighlighted)
                PlaySound(hoveredButton.audHighlightOverride != null
                    ? hoveredButton.audHighlightOverride
                    : audHighlight);

            hoveredButton.Highlight();
            _lastHighlighted = hoveredButton;

            if (InputManager.Instance.GetActionKeyDown(InputAction.Interact))
            {
                hoveredButton.Press();
                PlaySound(hoveredButton.audConfirmOverride != null
                    ? hoveredButton.audConfirmOverride
                    : audConfirm);
                _heldButton = hoveredButton;
                Debug.Log($"[PixelCursor] Pressed: {hoveredButton.gameObject.name}");
            }
        }
        else
        {
            _lastHighlighted = null;
        }

        if (_heldButton != null && InputManager.Instance.GetActionKeyUp(InputAction.Interact))
        {
            _heldButton.UnHold();
            _heldButton = null;
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Enable or disable pixel-cursor mode at runtime.</summary>
    public void SetPixelCursorActive(bool active)
    {
        pixelCursorActive = active;
        ApplyMouseMode();
    }

    public void Hide(bool hide) => SetHidden(hide);

    public void Blink(int frames)
    {
        SetHidden(true);
        _blinkFrames = frames;
    }

    public void Teleport(Vector2 canvasLocalPos)
    {
        _position.x = Mathf.Clamp(Mathf.Round(canvasLocalPos.x), minRange.x, maxRange.x);
        _position.y = Mathf.Clamp(Mathf.Round(canvasLocalPos.y), minRange.y, maxRange.y);
        ApplyPosition();
    }

    public void SetSprite(Sprite sprite) => cursorImage.sprite = sprite;
    public void SetColor(Color color) => cursorImage.color = color;

    public static Vector2 Movement => Instance != null ? Instance._movementThisFrame : Vector2.zero;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void ApplyPosition()
    {
        _localPosition.x = Mathf.Round(_position.x) - hotspot.x;
        _localPosition.y = Mathf.Round(_position.y) + hotspot.y;
        _rt.anchoredPosition = _localPosition;
    }

    private void ApplyMouseMode()
    {
        if (!pixelCursorActive)
        {
            cursorImage.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void SetHidden(bool hide)
    {
        _hidden = hide;
        cursorImage.enabled = !hide;
    }

    private void ClearHeldButton()
    {
        if (_heldButton != null) { _heldButton.UnHold(); _heldButton = null; }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}