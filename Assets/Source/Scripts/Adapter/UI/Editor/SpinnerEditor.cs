#if UNITY_EDITOR
namespace LB
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    /// <summary>
    /// Spinner editor.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Spinner), true)]
    public class SpinnerEditor : Editor
    {
        readonly Dictionary<string, SerializedProperty> serializedProperties = new Dictionary<string, SerializedProperty>();

        /// <summary>
        /// Properties.
        /// </summary>
        protected string[] properties = new string[]
        {
			// Spinner
			"ValueMin",
            "ValueMax",
            "ValueStep",
            "TextComponent",
            "SpinnerValue",
            "plusButton",
            "minusButton",
            "AllowHold",
            "HoldStartDelay",
            "HoldChangeDelay",
            "onPlusClick",
            "onMinusClick",
        };

        /// <summary>
        /// Init.
        /// </summary>
        protected virtual void OnEnable()
        {
            Array.ForEach(properties, x => serializedProperties.Add(x, serializedObject.FindProperty(x)));
        }

        /// <summary>
        /// Draw inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // ValidateTargets();

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedProperties["ValueMin"], true);
            EditorGUILayout.PropertyField(serializedProperties["ValueMax"], true);
            EditorGUILayout.PropertyField(serializedProperties["ValueStep"], true);

            EditorGUILayout.PropertyField(serializedProperties["TextComponent"], true);
            EditorGUILayout.PropertyField(serializedProperties["SpinnerValue"], true);

            EditorGUILayout.PropertyField(serializedProperties["plusButton"], true);
            EditorGUILayout.PropertyField(serializedProperties["minusButton"], true);

            EditorGUILayout.PropertyField(serializedProperties["AllowHold"], true);
            EditorGUILayout.PropertyField(serializedProperties["HoldStartDelay"], true);
            EditorGUILayout.PropertyField(serializedProperties["HoldChangeDelay"], true);

            EditorGUILayout.PropertyField(serializedProperties["onPlusClick"]);
            EditorGUILayout.PropertyField(serializedProperties["onMinusClick"]);

            serializedObject.ApplyModifiedProperties();

            // ValidateTargets();
        }
    }
}
#endif