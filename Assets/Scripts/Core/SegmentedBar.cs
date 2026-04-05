using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A segmented bar UI component. Each child Image segment is binary:
/// it shows either filledSprite or unfilledSprite based on the current Value.
/// Works in the Editor (via [ExecuteAlways]) and at runtime.
/// </summary>
[ExecuteAlways]
[AddComponentMenu("UI/Segmented Bar")]
public class SegmentedBar : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Sprites
    // ─────────────────────────────────────────────

    [BoxGroup("Sprites")]
    [Required]
    [Tooltip("Sprite shown on segments that are filled.")]
    public Sprite filledSprite;

    [BoxGroup("Sprites")]
    [Required]
    [Tooltip("Sprite shown on segments that are unfilled.")]
    public Sprite unfilledSprite;

    // ─────────────────────────────────────────────
    //  Value
    // ─────────────────────────────────────────────

    [BoxGroup("Value")]
    [Tooltip("The minimum allowed value of the bar.")]
    [OnValueChanged(nameof(Refresh))]
    [SerializeField] private float _minValue = 0f;

    [BoxGroup("Value")]
    [Tooltip("The maximum allowed value of the bar.")]
    [OnValueChanged(nameof(Refresh))]
    [SerializeField] private float _maxValue = 10f;

    /// <summary>The minimum allowed value. Clamped to be no greater than MaxValue.</summary>
    public float MinValue
    {
        get => _minValue;
        set
        {
            _minValue = Mathf.Min(value, _maxValue);
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
            Refresh();
        }
    }

    /// <summary>The maximum allowed value. Clamped to be no less than MinValue.</summary>
    public float MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = Mathf.Max(value, _minValue);
            _value = Mathf.Clamp(_value, _minValue, _maxValue);
            Refresh();
        }
    }

    [BoxGroup("Value")]
    [OnValueChanged(nameof(Refresh))]
    [Tooltip("Current value between MinValue and MaxValue.")]
    [SerializeField] private float _value = 10f;

    /// <summary>Gets or sets the current value (MinValue–MaxValue). Refreshes the bar immediately.</summary>
    public float Value
    {
        get => _value;
        set
        {
            float clamped = Mathf.Clamp(value, _minValue, _maxValue);
            if (Mathf.Approximately(clamped, _value)) return;

            _value = clamped;
            Refresh();
            if (Application.isPlaying)
                onValueChanged.Invoke(_value);
        }
    }

    // ─────────────────────────────────────────────
    //  Events
    // ─────────────────────────────────────────────

    [BoxGroup("Events")]
    [Tooltip("Invoked at runtime whenever Value changes. Passes the new value (0-1).")]
    public UnityEvent<float> onValueChanged = new UnityEvent<float>();

    // ─────────────────────────────────────────────
    //  Bars
    // ─────────────────────────────────────────────

    [BoxGroup("Bars")]
    [Tooltip("Parent whose direct Image children are the segments. Defaults to this GameObject.")]
    public RectTransform barsContainer;

    [BoxGroup("Bars")]
    [ReadOnly]
    [Tooltip("Auto-discovered Image segments (read-only).")]
    [SerializeField] private List<Image> _segments = new List<Image>();

    // ─────────────────────────────────────────────
    //  Editor Buttons
    // ─────────────────────────────────────────────

    [Button("Discover Segments")]
    public void DiscoverSegments()
    {
        var container = barsContainer != null ? barsContainer : (RectTransform)transform;
        _segments.Clear();

        foreach (Transform child in container)
        {
            if (child.TryGetComponent<Image>(out var img))
                _segments.Add(img);
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
        Refresh();
    }

    [Button("Refresh Display")]
    public void Refresh()
    {
        if (_segments == null || _segments.Count == 0)
            DiscoverSegments();

        if (_segments == null || _segments.Count == 0 || filledSprite == null || unfilledSprite == null)
            return;

        int total = _segments.Count;
        float range = _maxValue - _minValue;
        float normalised = range > 0f ? (_value - _minValue) / range : 0f;
        int filledCount = Mathf.RoundToInt(normalised * total);

        for (int i = 0; i < total; i++)
        {
            Image seg = _segments[i];
            if (seg == null) continue;

            Sprite chosen = i < filledCount ? filledSprite : unfilledSprite;

            if (seg.sprite != chosen)
                seg.sprite = chosen;
        }
    }

    // ─────────────────────────────────────────────
    //  Convenience API
    // ─────────────────────────────────────────────

    /// <summary>Set value from a current/max pair (e.g. HP). Maps onto [MinValue, MaxValue].</summary>
    public void SetValue(float current, float max)
    {
        float normalised = max > 0f ? current / max : 0f;
        Value = _minValue + Mathf.Clamp01(normalised) * (_maxValue - _minValue);
    }
    /// <summary>Returns the number of segments currently showing the filled sprite.</summary>
    public int FilledSegmentCount
    {
        get
        {
            if (_segments == null) return 0;
            int count = 0;
            foreach (var seg in _segments)
                if (seg != null && seg.sprite == filledSprite) count++;
            return count;
        }
    }

    public void ChangeValue(int num)
    {
        float range = _maxValue - _minValue;
        if (range <= 0f)
        {
            Debug.LogError("SegmentedBar: MinValue and MaxValue are equal — cannot change value.");
            return;
        }

        if (num == 0)
        {
            Debug.LogError("Input Number can't be equal to 0! (" + num.ToString() + ")");
            return;
        }

        int filled = FilledSegmentCount;
        int total = _segments != null ? _segments.Count : 0;

        if (num < 0 && filled == 0)
        {
            Debug.LogWarning("SegmentedBar: Already at 0 filled segments, cannot decrease further.");
            return;
        }

        if (num > 0 && filled == total)
        {
            Debug.LogWarning("SegmentedBar: All " + total + " segments already filled, cannot increase further.");
            return;
        }

        float step = range / Mathf.Max(total, 1);
        Value = Mathf.Clamp(_value + num * step, _minValue, _maxValue);
    }

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void OnEnable() => DiscoverSegments();

    private void OnValidate() => Refresh();

#if UNITY_EDITOR
    private void OnTransformChildrenChanged() => DiscoverSegments();
#endif
}