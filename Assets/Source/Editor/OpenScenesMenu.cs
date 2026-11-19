
using UnityEditor;
using UnityEditor.SceneManagement;
public static class OpenScenesMenu
{
    [MenuItem("OpenScenes/Main", priority = 10)]
    public static void Assets_Source_Main_unity() { OpenScene("Assets/Source/Scenes/Main.unity"); }

    [MenuItem("OpenScenes/UIEdit", priority = 11)]
    public static void Assets_Source_UIEdit_unity() { OpenScene("Assets/Source/Scenes/UIEdit.unity"); }
    // [MenuItem("Scenes/RegularEdit", priority = 10)]
    // public static void Assets_Source_RegularEdit_unity() { OpenScene("Assets/Source/Scenes/RegularEdit.unity"); }
    // [MenuItem("Scenes/UIEdit", priority = 10)]
    // public static void Assets_Source_UIEdit_unity() { OpenScene("Assets/Source/Scenes/UIEdit.unity"); }

    public static void OpenScene(string filename)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(filename);
    }
}
