using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/TMP Text Button", 31)]
    public class TMPTextButton : Selectable,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISubmitHandler
    {
        // ── Events ────────────────────────────────────────────────────────────

        [Serializable] public class ButtonClickedEvent : UnityEvent { }
        [Serializable] public class ButtonHoverEvent : UnityEvent<bool> { }

        [Header("Events")]
        [SerializeField] private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();
        [SerializeField] private ButtonHoverEvent m_OnHover = new ButtonHoverEvent();

        public ButtonClickedEvent onClick { get => m_OnClick; set => m_OnClick = value; }
        public ButtonHoverEvent onHover { get => m_OnHover; set => m_OnHover = value; }

        // ── TMP Reference ─────────────────────────────────────────────────────

        [Header("Text Reference")]
        [Tooltip("The TextMeshProUGUI label to drive. Auto-found on children if left empty.")]
        [SerializeField] private TextMeshProUGUI m_Label;

        public TextMeshProUGUI label
        {
            get
            {
                if (m_Label == null) m_Label = GetComponentInChildren<TextMeshProUGUI>();
                return m_Label;
            }
            set => m_Label = value;
        }

        // ── State Config ──────────────────────────────────────────────────────

        [Serializable]
        public class TextState
        {
            public Color color = Color.white;
            public FontStyles fontStyle = FontStyles.Normal;
            [Min(0f)]
            public float transitionDuration = 0.08f;
        }

        [Header("Text States")]
        [SerializeField] private TextState m_NormalState = new TextState { color = Color.white };
        [SerializeField] private TextState m_HoverState = new TextState { color = new Color(0.8f, 0.9f, 1f) };
        [SerializeField] private TextState m_PressedState = new TextState { color = new Color(0.6f, 0.75f, 1f), fontStyle = FontStyles.Bold };
        [SerializeField] private TextState m_DisabledState = new TextState { color = new Color(0.5f, 0.5f, 0.5f, 0.5f) };

        public TextState normalState { get => m_NormalState; set => m_NormalState = value; }
        public TextState hoverState { get => m_HoverState; set => m_HoverState = value; }
        public TextState pressedState { get => m_PressedState; set => m_PressedState = value; }
        public TextState disabledState { get => m_DisabledState; set => m_DisabledState = value; }

        // ── Private State ─────────────────────────────────────────────────────

        private bool _isHovered;
        private Coroutine _transitionCoroutine;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        protected override void Awake()
        {
            transition = Transition.None;
            targetGraphic = null;
            base.Awake();
        }

        protected override void OnEnable()
        {
            transition = Transition.None;
            targetGraphic = null;
            base.OnEnable();

            if (label != null)
                ApplyState(IsInteractable() ? m_NormalState : m_DisabledState, instant: true);
        }

        // ── Pointer ───────────────────────────────────────────────────────────

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (!IsInteractable()) return;
            _isHovered = true;
            m_OnHover.Invoke(true);
            ApplyState(m_HoverState);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            _isHovered = false;
            m_OnHover.Invoke(false);
            ApplyState(IsInteractable() ? m_NormalState : m_DisabledState);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsInteractable()) return;
            if (eventData.button == PointerEventData.InputButton.Left)
                ApplyState(m_PressedState);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (!IsInteractable()) return;
            ApplyState(_isHovered ? m_HoverState : m_NormalState);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            Press();
        }

        // ── Submit ────────────────────────────────────────────────────────────

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();
            if (!IsActive() || !IsInteractable()) return;
            ApplyState(m_PressedState);
            StartCoroutine(FinishSubmitRoutine());
        }

        private IEnumerator FinishSubmitRoutine()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            ApplyState(m_NormalState);
        }

        // ── State Transition Override ─────────────────────────────────────────

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (!gameObject.activeInHierarchy) return;

            var target = state switch
            {
                SelectionState.Highlighted => m_HoverState,
                SelectionState.Pressed => m_PressedState,
                SelectionState.Disabled => m_DisabledState,
                _ => m_NormalState
            };

            ApplyState(target, instant);
        }

        // ── Press ─────────────────────────────────────────────────────────────

        private void Press()
        {
            if (!IsActive() || !IsInteractable()) return;
            UISystemProfilerApi.AddMarker("TMPTextButton.onClick", this);
            m_OnClick.Invoke();
        }

        // ── Apply State ───────────────────────────────────────────────────────

        public void ApplyState(TextState state, bool instant = false)
        {
            if (state == null || label == null) return;
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);

            if (instant || state.transitionDuration <= 0f || !gameObject.activeInHierarchy)
            {
                SetVisuals(state.color, state.fontStyle);
                return;
            }

            _transitionCoroutine = StartCoroutine(TransitionRoutine(state));
        }

        private IEnumerator TransitionRoutine(TextState target)
        {
            Color fromColor = label.color;
            label.fontStyle = target.fontStyle; // can't lerp, snap it

            float elapsed = 0f;
            while (elapsed < target.transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / target.transitionDuration));
                label.color = Color.Lerp(fromColor, target.color, t);
                yield return null;
            }

            SetVisuals(target.color, target.fontStyle);
            _transitionCoroutine = null;
        }

        private void SetVisuals(Color color, FontStyles style)
        {
            label.color = color;
            label.fontStyle = style;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        public void SetText(string text) => label.text = text;
        public string GetText() => label.text;
        public void SimulateClick() => Press();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            transition = Transition.None;
            targetGraphic = null;
            base.OnValidate();
        }
#endif
    }
}