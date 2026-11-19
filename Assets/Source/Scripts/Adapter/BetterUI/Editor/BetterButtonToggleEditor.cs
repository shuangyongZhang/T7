
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Editor
{
    [CustomEditor(typeof(BetterButtonToggle)), CanEditMultipleObjects]
    public class BetterButtonToggleEditor : ButtonEditor
    {
        BetterElementHelper<Button, BetterButtonToggle> transitions =
            new BetterElementHelper<Button, BetterButtonToggle>();
        BetterElementHelper<Button, BetterButtonToggle> OnOffTransitions =
            new BetterElementHelper<Button, BetterButtonToggle>("betterToggleTransitions");
        int toggleTransCount = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BetterButtonToggle btnToggle = target as BetterButtonToggle;
            transitions.DrawGui(serializedObject);

            EditorGUILayout.Space();
            var groupProperty = serializedObject.FindProperty("buttonGroup");
            BetterButtonToggleGroup group = groupProperty.objectReferenceValue as BetterButtonToggleGroup;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(groupProperty);
            if (EditorGUI.EndChangeCheck())
            {
                EditorSceneManager.MarkSceneDirty(btnToggle.gameObject.scene);
                btnToggle.group = group;
            }

            var isClickProperty = serializedObject.FindProperty("isClick");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isClickProperty);
            if (EditorGUI.EndChangeCheck())
            {
                EditorSceneManager.MarkSceneDirty(btnToggle.gameObject.scene);
                btnToggle.IsClick = isClickProperty.boolValue;

                if (group != null && btnToggle.IsActive() && btnToggle.IsInteractable())
                {
                    if (btnToggle.IsClick || (!group.AnyButtonsOn() && !group.allowSwitchOff))
                    {
                        btnToggle.IsClick = true;
                        group.NotifyButtonOn(btnToggle);
                    }
                }
            }
            OnOffTransitions.DrawGui(serializedObject);

            serializedObject.ApplyModifiedProperties();
        }

        // [MenuItem("CONTEXT/Button/♠ Make Better")]
        // public static void MakeBetter(MenuCommand command)
        // {
        //     Button btn = command.context as Button;
        //     Betterizer.MakeBetter<Button, BetterButtonToggle>(btn);
        // }
    }
}
