using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages a toggle's visual state — checkmark, disable cover, and hotspot.
/// Pair with a StandardButton on the hotspot child to hook into PixelCursor.
/// </summary>
public class ToggleControl : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Visuals")]
    [SerializeField] private GameObject checkmark;
    [SerializeField] private GameObject disableCover;

    [Header("Hotspot")]
    [Tooltip("The child GameObject that acts as the clickable area. Must have a StandardButton on it.")]
    [SerializeField] private GameObject hotspot;

    [Header("Initial State")]
    [SerializeField] private bool startValue = false;

    [Header("Events")]
    public UnityEvent<bool> OnValueChanged;

    // ─── Public State ─────────────────────────────────────────────────────────
    public bool Value => _value;
    public bool IsDisabled => _disabled;

    // ─── Private ──────────────────────────────────────────────────────────────
    private bool _value;
    private bool _disabled;
    private StandardButton _button;

    // ─── Unity Messages ───────────────────────────────────────────────────────
    private void Awake()
    {
        _value = startValue;

        if (hotspot != null)
        {
            _button = hotspot.GetComponent<StandardButton>();
            if (_button != null)
                _button.OnPress.AddListener(Toggle);
        }

        RefreshVisuals();
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.OnPress.RemoveListener(Toggle);
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>Flip the current value.</summary>
    public void Toggle()
    {
        if (_disabled) return;
        SetValue(!_value);
    }

    /// <summary>Set value and update visuals. Respects disabled state.</summary>
    public void Set(bool value)
    {
        if (_disabled) return;
        SetValue(value);
    }

    /// <summary>Set value without firing OnValueChanged.</summary>
    public void SetSilent(bool value)
    {
        _value = value;
        RefreshVisuals();
    }

    /// <summary>Enable or disable the toggle. When disabled, the cover shows and hotspot hides.</summary>
    public void SetDisabled(bool disabled)
    {
        _disabled = disabled;
        if (disableCover != null) disableCover.SetActive(disabled);
        if (hotspot != null) hotspot.SetActive(!disabled);
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────
    private void SetValue(bool value)
    {
        _value = value;
        RefreshVisuals();
        OnValueChanged.Invoke(_value);
    }

    private void RefreshVisuals()
    {
        if (checkmark != null) checkmark.SetActive(_value);
    }
}