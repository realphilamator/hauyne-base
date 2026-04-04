using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [Range(0f, 1f)]
    [OnValueChanged(nameof(Refresh))]
    [Tooltip("Fill amount from 0 (empty) to 1 (completely full).")]
    [SerializeField] private float _value = 1f;

    /// <summary>Gets or sets the fill value (0-1). Refreshes the bar immediately.</summary>
    public float Value
    {
        get => _value;
        set
        {
            _value = Mathf.Clamp01(value);
            Refresh();
        }
    }

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
        // Round to nearest so 0.65 x 10 = 6.5 -> 7 filled segments
        int filledCount = Mathf.RoundToInt(_value * total);

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

    /// <summary>Set value from a current/max pair (e.g. HP).</summary>
    public void SetValue(float current, float max)
    {
        Value = max > 0f ? current / max : 0f;
    }

    /// <summary>Animate toward a target value over time. Use with StartCoroutine.</summary>
    public System.Collections.IEnumerator AnimateTo(float targetValue, float duration)
    {
        float start = _value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Value = Mathf.Lerp(start, targetValue, elapsed / duration);
            yield return null;
        }

        Value = targetValue;
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