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
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class BuildWindow : EditorWindow
{
    // --- UI 状态变量 ---
    private BuildTarget selectedTarget;
    private string versionString;
    private VersionType versionType;
    private string dateString;
    private bool needCompress;
    private Vector2 asmdefListScrollPos; // 用于滚动视图的位置

    //Build Settings
    private bool developmentBuild;
    private bool allowDebugging;
    private bool compressWithLz4;
    private bool buildAppBundle; // For Android

    private Action<BuildParameters> onStartBuild;

    // --- 数据 ---
    private List<AsmdefInfo> _availableAsmdefs;

    public static void ShowWindow(Action<BuildParameters> buildAction)
    {
        BuildWindow window = GetWindow<BuildWindow>(true, "构建编译器", true);
        window.onStartBuild = buildAction;
        window.minSize = new Vector2(380, 500); // 调整窗口大小以容纳列表
        window.maxSize = new Vector2(380, 660);

        window.Show();
    }

    private void OnEnable()
    {
        // 初始化默认值
        selectedTarget = EditorUserBuildSettings.activeBuildTarget;
        versionString = PlayerSettings.bundleVersion;
        dateString = DateTime.Now.ToString("yyyy-MM-dd");
        versionType = VersionType.Release;
        LoadBuildSettings();
        // 查找项目中所有的 .asmdef 文件
        _availableAsmdefs = AsmdefVersionManager.GetAllVersionedAsmdefs();
    }

    void OnGUI()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("构建参数设置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 1. 平台选择
        selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup("构建平台:", selectedTarget);

        // 2. 版本号输入
        versionString = EditorGUILayout.TextField(new GUIContent("版本号:", "格式必须为X.X.X.X"), versionString);

        // 3. 版本类型选择
        versionType = (VersionType)EditorGUILayout.EnumPopup("版本类型:", versionType);

        // 4. 日期输入
        dateString = EditorGUILayout.TextField(new GUIContent("构建日期:", "格式: yyyy-MM-dd"), dateString);

        // 5. 是否自动压缩
        needCompress = EditorGUILayout.Toggle(new GUIContent("自动压缩:", "构建目录有多个文件则自动打压缩包zip。否则跳过"), needCompress);
        EditorGUILayout.EndVertical();

        // --- Build Settings 区域 ---
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("平台构建设置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUI.BeginChangeCheck();

        developmentBuild = EditorGUILayout.Toggle(new GUIContent("Development Build", "勾选以启用开发构建，包含分析器和调试信息。"), developmentBuild);

        // 只有在 Development Build 开启时，才允许修改调试选项
        EditorGUI.BeginDisabledGroup(!developmentBuild);
        allowDebugging = EditorGUILayout.Toggle(new GUIContent("Script Debugging", "允许附加脚本调试器。"), allowDebugging);
        EditorGUI.EndDisabledGroup();

        // LZ4压缩选项
        // compressWithLz4 = EditorGUILayout.Toggle(new GUIContent("Compress With LZ4", "使用LZ4压缩资源包，可以加快加载速度但包体会稍大。"), compressWithLz4);

        // 仅当目标平台是Android时，才显示AAB选项
        if (selectedTarget == BuildTarget.Android)
        {
            buildAppBundle = EditorGUILayout.Toggle(new GUIContent("Build App Bundle (AAB)", "构建为Google Play需要的AAB格式，而不是APK。"), buildAppBundle);
        }

        if (EditorGUI.EndChangeCheck())
        {
            ApplyBuildSettings();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- 按钮区域 ---
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("仅编译 DLL", GUILayout.Height(35)))
        {
            if (EditorUtility.DisplayDialog("编译DLL", $"确定要为当前 {selectedTarget} 平台编译DLL吗?", "确定", "取消"))
            {
                BuildTools.DoCompileDll(selectedTarget);
            }
        }
        if (GUILayout.Button("完整构建", GUILayout.Height(35)))
        {
            if (ValidateInputs())
            {
                var buildParams = new BuildParameters
                {
                    Target = selectedTarget,
                    Version = versionString,
                    VersionType = versionType,
                    DateString = dateString,
                    NeedCompress = needCompress,
                    DevelopmentBuild = developmentBuild,
                    AllowDebugging = allowDebugging,
                    CompressWithLz4 = compressWithLz4,
                    BuildAppBundle = buildAppBundle
                };

                if (EditorUtility.DisplayDialog("构建发行包", $"确定要为平台 {selectedTarget} 构建完整发行包吗?", "确定", "取消"))
                {
                    onStartBuild?.Invoke(buildParams);
                    this.Close();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        DrawAsmdef();
    }
    /// <summary>
    /// 从 EditorUserBuildSettings 加载当前的构建设置到UI变量。
    /// </summary>
    private void LoadBuildSettings()
    {
        developmentBuild = EditorUserBuildSettings.development;
        allowDebugging = EditorUserBuildSettings.allowDebugging;
        buildAppBundle = EditorUserBuildSettings.buildAppBundle;
    }

    /// <summary>
    /// 将UI变量中的设置应用回 EditorUserBuildSettings。
    /// </summary>
    private void ApplyBuildSettings()
    {
        EditorUserBuildSettings.development = developmentBuild;
        EditorUserBuildSettings.allowDebugging = allowDebugging;

        // 只有当目标是Android时才设置AAB选项
        if (selectedTarget == BuildTarget.Android)
        {
            EditorUserBuildSettings.buildAppBundle = buildAppBundle;
        }

        Debug.Log("Build settings updated from BuildWindow.");
    }

    private void DrawAsmdef()
    {
        // --- Asmdef 版本信息展示区 ---
        EditorGUILayout.LabelField("项目内 Asmdef 版本参考", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));

        if (_availableAsmdefs != null && _availableAsmdefs.Count > 0)
        {
            // 绘制表头
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("模块名称 (点击定位)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("版本号", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            asmdefListScrollPos = EditorGUILayout.BeginScrollView(asmdefListScrollPos);
            foreach (var info in _availableAsmdefs)
            {
                EditorGUILayout.BeginHorizontal();


                GUIStyle linkStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = new Color(0.298f, 0.49f, 1.0f) },
                    hover = { textColor = new Color(0.1f, 0.8f, 0.8f) },
                    stretchWidth = true
                };

                if (GUILayout.Button(new GUIContent(info.name, "点击在 Project 窗口中定位此文件"), linkStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    //定位文件
                    PingAsset(info.Path);
                }

                EditorGUILayout.SelectableLabel(info.version, GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));

                if (GUILayout.Button("打开", GUILayout.Width(60)))
                {
                    OpenFileWithDefaultEditor(info.Path);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("项目中未找到带有 'version' 字段的 .asmdef 文件。");
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    private bool ValidateInputs()
    {
        // 版本号格式验证X.X.X.X
        string versionPattern = @"^(\d+\.){3}\d+$";
        if (!Regex.IsMatch(versionString, versionPattern))
        {
            EditorUtility.DisplayDialog("无效的版本号", "版本号格式应为X.X.X.X (例如1.0.2.5)。", "好的");
            return false;
        }

        string datePattern = @"^\d{4}-\d{2}-\d{2}$";
        if (!Regex.IsMatch(dateString, datePattern))
        {
            EditorUtility.DisplayDialog("无效的日期", "日期格式必须为 yyyy-MM-dd (例如: 2025-06-20)。", "好的");
            return false;
        }
        return true;
    }
    //--- 辅助方法 ---

    /// <summary>
    /// 使用系统默认的应用程序打开指定路径的文件。
    /// </summary>
    /// <param name="assetPath">文件的项目相对路径 (e.g., "Assets/Scripts/MyModule.asmdef")</param>
    private void OpenFileWithDefaultEditor(string assetPath)
    {
        // 1. 将 Unity 的项目相对路径转换为系统的绝对路径
        string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));

        if (File.Exists(fullPath))
        {
            // 2.打开文件
            System.Diagnostics.Process.Start(fullPath);
        }
        else
        {
            Debug.LogError($"无法打开文件，因为它不存在: {fullPath}");
            EditorUtility.DisplayDialog("错误", $"文件路径不存在：\n{fullPath}", "好的");
        }
    }


    /// <summary>
    /// 在 Unity 的 Project 窗口中高亮（Ping）一个资源。
    /// </summary>
    /// <param name="assetPath">资源的相对路径 (e.g., "Assets/...")</param>
    private void PingAsset(string assetPath)
    {
        // 1. 根据路径加载资源对象。对于 .asmdef，它是一种 Object。
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

        if (asset != null)
        {
            // 2. 高亮这个对象
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }
        else
        {
            Debug.LogWarning($"无法定位资源，因为它无法被加载: {assetPath}");
        }
    }
}