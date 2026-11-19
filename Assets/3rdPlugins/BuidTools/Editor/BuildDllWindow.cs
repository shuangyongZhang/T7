/****************************************************************************
 * Copyright 2025 YQY (sho)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;
public class BuildDllWindow : EditorWindow
{
    // 默认选中的平台
    private BuildTarget selectedTarget = BuildTarget.StandaloneWindows64;
    private Func<BuildTarget, bool> onCompile;

    public static void ShowWindow(Func<BuildTarget, bool> compileAction)
    {
        BuildDllWindow window = GetWindow<BuildDllWindow>(true, "选择编译平台", true);
        window.onCompile = compileAction;
        window.minSize = new Vector2(350, 120);
        window.maxSize = new Vector2(350, 120);
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("请选择DLL编译的目标平台:", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup("构建目标：", selectedTarget);

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("编译", GUILayout.Width(100), GUILayout.Height(30)))
        {
            onCompile?.Invoke(selectedTarget);
            this.Close();
        }

        if (GUILayout.Button("取消", GUILayout.Width(100), GUILayout.Height(30)))
        {
            this.Close();
        }

        EditorGUILayout.EndHorizontal();
    }
}