#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    /// <summary>
    /// Custom Inspector for TMPTextButton.
    /// Hides the entire Selectable Transition section — we manage visuals ourselves.
    /// </summary>
    [CustomEditor(typeof(TMPTextButton), true)]
    [CanEditMultipleObjects]
    public class TMPTextButtonEditor : SelectableEditor
    {
        // Properties we want to show from the TMPTextButton itself
        SerializedProperty m_OnClick;
        SerializedProperty m_OnHover;
        SerializedProperty m_Label;
        SerializedProperty m_NormalState;
        SerializedProperty m_HoverState;
        SerializedProperty m_PressedState;
        SerializedProperty m_DisabledState;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClick       = serializedObject.FindProperty("m_OnClick");
            m_OnHover       = serializedObject.FindProperty("m_OnHover");
            m_Label         = serializedObject.FindProperty("m_Label");
            m_NormalState   = serializedObject.FindProperty("m_NormalState");
            m_HoverState    = serializedObject.FindProperty("m_HoverState");
            m_PressedState  = serializedObject.FindProperty("m_PressedState");
            m_DisabledState = serializedObject.FindProperty("m_DisabledState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ── Interactable only (from Selectable) ──────────────────────────
            // We deliberately skip DrawDefaultInspector / base.OnInspectorGUI
            // so the Transition / TargetGraphic / Colors block never appears.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Interactable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Navigation"));

            EditorGUILayout.Space();

            // ── TMP Label ────────────────────────────────────────────────────
            EditorGUILayout.LabelField("Text Reference", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Label);

            EditorGUILayout.Space();

            // ── States ───────────────────────────────────────────────────────
            EditorGUILayout.LabelField("Visual States", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_NormalState,   true);
            EditorGUILayout.PropertyField(m_HoverState,    true);
            EditorGUILayout.PropertyField(m_PressedState,  true);
            EditorGUILayout.PropertyField(m_DisabledState, true);

            EditorGUILayout.Space();

            // ── Events ───────────────────────────────────────────────────────
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnClick);
            EditorGUILayout.PropertyField(m_OnHover);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
