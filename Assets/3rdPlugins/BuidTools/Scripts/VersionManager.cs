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

/// <summary>
/// 运行时提供构建版本信息。
/// </summary>
public static class VersionManager
{
    // --- 私有数据 ---
    private static BuildInfoData _buildInfo;
    private static bool _isLoaded = false;

    /// <summary>
    /// 获取完整的版本号字符串，例如 "1.0.0.1"。
    /// </summary>
    public static string Version => _isLoaded ? _buildInfo.version : "N/A";

    /// <summary>
    /// 获取版本类型，例如 "Alpha", "Beta", "Release"。
    /// </summary>
    public static string VersionType => _isLoaded ? _buildInfo.versionType : "N/A";

    /// <summary>
    /// 获取构建日期字符串，例如 "2025-10-26"。
    /// </summary>
    public static string BuildDate => _isLoaded ? _buildInfo.buildDate : "N/A";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_isLoaded) return;

        // 1. 加载构建信息
        LoadBuildInfo();

        // 2. 添加版本显示窗口
        AddVersionDisplayWindow();
    }

    private static void LoadBuildInfo()
    {
        if (_isLoaded) return;

        var textAsset = Resources.Load<TextAsset>("build_info.json");

        if (textAsset != null)
        {
            _buildInfo = JsonUtility.FromJson<BuildInfoData>(textAsset.text);
            _isLoaded = true;
        }
        else
        {
            _buildInfo = new BuildInfoData(); // 创建一个空的，防止 NullReferenceException
            Debug.LogError("VersionManager: Failed to load 'build_info.json' from Resources. Version info will be unavailable.");
        }

    }

    private static void AddVersionDisplayWindow()
    {
        if (VersionType != "Release")
        {
#if UNITY_2020_2_OR_NEWER
            VersionDisplayWindow window = GameObject.FindAnyObjectByType<VersionDisplayWindow>();
#else
            VersionDisplayWindow window = GameObject.FindObjectOfType<VersionDisplayWindow>();
#endif
            if (window != null)
            {
                GameObject.Destroy(window.gameObject);
            }
            var go = new GameObject("VersionDisplayWindow");
            GameObject.DontDestroyOnLoad(go);
            var versionDisplay = go.AddComponent<VersionDisplayWindow>();
            versionDisplay.showToggleButton = true;
        }
    }
}