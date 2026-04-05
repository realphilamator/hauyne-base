using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NaughtyAttributes;

/// <summary>
/// Multipurpose cursor-driven button. Tag this GameObject with "Button" so
/// PixelCursor can detect it via GraphicRaycaster.
/// </summary>
public class StandardButton : MonoBehaviour
{
    // ─── Mode ─────────────────────────────────────────────────────────────────

    public enum ButtonMode { SpriteSwap, TMPText, Toggle, Animation }

    [BoxGroup("General")]
    public ButtonMode mode = ButtonMode.SpriteSwap;

    [BoxGroup("General")]
    public bool interactable = true;

    // ─── Audio Overrides ──────────────────────────────────────────────────────

    [BoxGroup("Audio Overrides")]
    [Tooltip("Overrides PixelCursor's default highlight sound for this button.")]
    public AudioClip audHighlightOverride;

    [BoxGroup("Audio Overrides")]
    [Tooltip("Overrides PixelCursor's default confirm sound for this button.")]
    public AudioClip audConfirmOverride;

    // ─── Events ───────────────────────────────────────────────────────────────

    [BoxGroup("Events")]
    public UnityEvent OnPress;

    [BoxGroup("Events")]
    public UnityEvent OnRelease;

    [BoxGroup("Events")]
    public UnityEvent OnHighlight;

    [BoxGroup("Events")]
    public UnityEvent OffHighlight;

    // ─── Sprite Swap ──────────────────────────────────────────────────────────

    [BoxGroup("Sprite Swap")]
    [ShowIf("IsSpriteSwap")]
    public Image targetImage;

    [BoxGroup("Sprite Swap")]
    [ShowIf("IsSpriteSwap")]
    public Sprite normalSprite;

    [BoxGroup("Sprite Swap")]
    [ShowIf("IsSpriteSwap")]
    public Sprite highlightedSprite;

    [BoxGroup("Sprite Swap")]
    [ShowIf("IsSpriteSwap")]
    public Sprite heldSprite;

    // ─── TMP Text ─────────────────────────────────────────────────────────────

    [Serializable]
    public class TextState
    {
        public Color color = Color.white;
        public FontStyles fontStyle = FontStyles.Normal;
        [Min(0f)] public float transitionDuration = 0.08f;
    }

    [BoxGroup("TMP Text")]
    [ShowIf("IsTMPText")]
    public TMP_Text label;

    [BoxGroup("TMP Text")]
    [ShowIf("IsTMPText")]
    public TextState normalState   = new TextState { color = Color.white };

    [BoxGroup("TMP Text")]
    [ShowIf("IsTMPText")]
    public TextState highlightState = new TextState { color = new Color(0.8f, 0.9f, 1f) };

    [BoxGroup("TMP Text")]
    [ShowIf("IsTMPText")]
    public TextState pressedState  = new TextState { color = new Color(0.6f, 0.75f, 1f), fontStyle = FontStyles.Bold };

    [BoxGroup("TMP Text")]
    [ShowIf("IsTMPText")]
    public TextState disabledState = new TextState { color = new Color(0.5f, 0.5f, 0.5f, 0.5f) };

    // ─── Toggle ───────────────────────────────────────────────────────────────

    [BoxGroup("Toggle")]
    [ShowIf("IsToggle")]
    public bool toggleState = false;

    [BoxGroup("Toggle")]
    [ShowIf("IsToggle")]
    public Image toggleImage;

    [BoxGroup("Toggle")]
    [ShowIf("IsToggle")]
    public Sprite toggleOffSprite;

    [BoxGroup("Toggle")]
    [ShowIf("IsToggle")]
    public Sprite toggleOnSprite;

    [BoxGroup("Toggle")]
    [ShowIf("IsToggle")]
    public UnityEvent<bool> OnToggleChanged;

    // ─── Animation ────────────────────────────────────────────────────────────

    [BoxGroup("Animation")]
    [ShowIf("IsAnimation")]
    public Animator animator;

    [BoxGroup("Animation")]
    [ShowIf("IsAnimation")]
    public string highlightAnimation;

    [BoxGroup("Animation")]
    [ShowIf("IsAnimation")]
    public string unhighlightAnimation;

    [BoxGroup("Animation")]
    [ShowIf("IsAnimation")]
    public string pressAnimation;

    // ─── State ────────────────────────────────────────────────────────────────

    public bool WasHighlighted => _wasHighlighted;

    private bool _highlighted;
    private bool _wasHighlighted;
    private bool _held;
    private Coroutine _transitionCoroutine;

    // ─── Unity Messages ───────────────────────────────────────────────────────

    private void OnEnable()
    {
        _highlighted    = false;
        _wasHighlighted = false;
        _held           = false;
        RefreshVisuals(instant: true);
    }

    private void OnDisable()
    {
        if (_highlighted) Unhighlight();
        _highlighted    = false;
        _wasHighlighted = false;
    }

    private void Update()
    {
        // Each frame PixelCursor calls Highlight() if hovered.
        // If it wasn't called this frame, we unhighlight.
        if (!_highlighted && _wasHighlighted)
        {
            _wasHighlighted = false;
            Unhighlight();
        }
        _highlighted = false;
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public virtual void Highlight()
    {
        if (!interactable) return;

        if (!_wasHighlighted)
        {
            OnHighlight.Invoke();

            switch (mode)
            {
                case ButtonMode.SpriteSwap:
                    if (targetImage != null && highlightedSprite != null && !_held)
                        targetImage.sprite = highlightedSprite;
                    break;

                case ButtonMode.TMPText:
                    ApplyTextState(highlightState);
                    break;

                case ButtonMode.Animation:
                    if (animator != null && !string.IsNullOrEmpty(highlightAnimation))
                    {
                        animator.Play(highlightAnimation, -1, 0f);
                        animator.speed = 1f;
                    }
                    break;
            }
        }

        _wasHighlighted = true;
        _highlighted    = true;
    }

    public virtual void Press()
    {
        if (!interactable) return;

        _held = true;
        OnPress.Invoke();

        switch (mode)
        {
            case ButtonMode.SpriteSwap:
                if (targetImage != null && heldSprite != null)
                    targetImage.sprite = heldSprite;
                break;

            case ButtonMode.TMPText:
                ApplyTextState(pressedState);
                break;

            case ButtonMode.Toggle:
                toggleState = !toggleState;
                RefreshToggleSprite();
                OnToggleChanged.Invoke(toggleState);
                break;

            case ButtonMode.Animation:
                if (animator != null && !string.IsNullOrEmpty(pressAnimation))
                {
                    animator.Play(pressAnimation, -1, 0f);
                    animator.speed = 1f;
                }
                break;
        }
    }

    public virtual void UnHold()
    {
        _held = false;
        OnRelease.Invoke();

        switch (mode)
        {
            case ButtonMode.SpriteSwap:
                if (targetImage != null)
                    targetImage.sprite = _highlighted ? highlightedSprite : normalSprite;
                break;

            case ButtonMode.TMPText:
                ApplyTextState(_highlighted ? highlightState : normalState);
                break;
        }
    }

    /// <summary>Force set toggle state without firing OnToggleChanged.</summary>
    public void SetToggleSilent(bool value)
    {
        toggleState = value;
        RefreshToggleSprite();
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
        RefreshVisuals(instant: true);
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private void Unhighlight()
    {
        OffHighlight.Invoke();

        switch (mode)
        {
            case ButtonMode.SpriteSwap:
                if (targetImage != null && !_held)
                    targetImage.sprite = normalSprite;
                break;

            case ButtonMode.TMPText:
                ApplyTextState(interactable ? normalState : disabledState);
                break;

            case ButtonMode.Animation:
                if (animator != null)
                {
                    if (!string.IsNullOrEmpty(unhighlightAnimation))
                    {
                        animator.Play(unhighlightAnimation, -1, 0f);
                        animator.speed = 1f;
                    }
                    else if (!string.IsNullOrEmpty(highlightAnimation))
                    {
                        // Freeze the highlight anim at frame 0 as a fallback
                        animator.Play(highlightAnimation, -1, 0f);
                        animator.speed = 0f;
                    }
                }
                break;
        }
    }

    private void RefreshVisuals(bool instant = false)
    {
        switch (mode)
        {
            case ButtonMode.SpriteSwap:
                if (targetImage != null && normalSprite != null)
                    targetImage.sprite = normalSprite;
                break;

            case ButtonMode.TMPText:
                ApplyTextState(interactable ? normalState : disabledState, instant);
                break;

            case ButtonMode.Toggle:
                RefreshToggleSprite();
                break;
        }
    }

    private void RefreshToggleSprite()
    {
        if (toggleImage == null) return;
        toggleImage.sprite = toggleState ? toggleOnSprite : toggleOffSprite;
    }

    private void ApplyTextState(TextState state, bool instant = false)
    {
        if (state == null || label == null) return;
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);

        if (instant || state.transitionDuration <= 0f || !gameObject.activeInHierarchy)
        {
            label.color     = state.color;
            label.fontStyle = state.fontStyle;
            return;
        }

        _transitionCoroutine = StartCoroutine(TextTransitionRoutine(state));
    }

    private IEnumerator TextTransitionRoutine(TextState target)
    {
        Color fromColor  = label.color;
        label.fontStyle  = target.fontStyle;

        float elapsed = 0f;
        while (elapsed < target.transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / target.transitionDuration));
            label.color = Color.Lerp(fromColor, target.color, t);
            yield return null;
        }

        label.color          = target.color;
        label.fontStyle      = target.fontStyle;
        _transitionCoroutine = null;
    }

    // ─── NaughtyAttributes Conditions ────────────────────────────────────────

    private bool IsSpriteSwap()  => mode == ButtonMode.SpriteSwap;
    private bool IsTMPText()     => mode == ButtonMode.TMPText;
    private bool IsToggle()      => mode == ButtonMode.Toggle;
    private bool IsAnimation()   => mode == ButtonMode.Animation;
}
