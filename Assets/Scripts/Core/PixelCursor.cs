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
    private StandardButton _heldButton;
    private StandardButton _lastHighlighted;

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

        Debug.Log($"[PixelCursor] Awake | canvas={_canvas.name}");
    }

    private void Update()
    {
        if (_blinkFrames > 0)
        {
            _blinkFrames--;
            if (_blinkFrames <= 0)
                SetHidden(false);
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (cursorImage.enabled) cursorImage.enabled = false;
            return;
        }

        if (!_hidden && !cursorImage.enabled) cursorImage.enabled = true;
        Cursor.visible = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            Input.mousePosition,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            out Vector2 localPoint
        );

        _position.x = Mathf.Clamp(Mathf.Round(localPoint.x), minRange.x, maxRange.x);
        _position.y = Mathf.Clamp(Mathf.Round(localPoint.y), minRange.y, maxRange.y);

        ApplyPosition();

        if (_hidden) { ClearHeldButton(); return; }
        if (graphicRaycaster == null || _eventSystem == null) { ClearHeldButton(); return; }

        _pointerEventData = new PointerEventData(_eventSystem);
        _pointerEventData.position = Input.mousePosition;
        _pointerEventData.pressPosition = Input.mousePosition;

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

    private void ApplyPosition()
    {
        _localPosition.x = _position.x - hotspot.x;
        _localPosition.y = _position.y + hotspot.y;
        _rt.anchoredPosition = _localPosition;
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