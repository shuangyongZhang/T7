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
using System.Collections.Generic;

/// <summary>
/// 在运行时显示一个包含所有带版本号的程序集（Asmdef）信息的窗口。
/// 该窗口显示了每个程序集的名称、版本号。
/// </summary>
public class VersionDisplayWindow : MonoBehaviour
{
    // --- Inspector Fields ---
    [Header("键盘快捷键设置")]
    [Tooltip("用于切换窗口显示/隐藏的键盘快捷键")]
    public KeyCode toggleKey = KeyCode.F12;

    [Header("移动端/触摸屏设置")]
    [Tooltip("在屏幕上显示一个可点击的按钮来切换窗口")]
    public bool showToggleButton = false;

    // --- Private UI Rects ---
    private Rect windowRect;
    private Rect toggleButtonRect;
    private readonly string toggleButtonText = "版本信息";


    // --- Private UI and Data Fields ---
    private bool _isWindowVisible = false;
    private Vector2 _scrollPosition;
    private List<AsmdefInfo> _versionInfos;
    private Dictionary<string, string> validVersions;


    // --- Private Styling and State Fields ---
    private GUIStyle _windowStyle;
    private GUIStyle _headerStyle;
    private GUIStyle _cellStyle;
    private GUIStyle _separatorStyle;
    private GUIStyle _buttonStyle;
    private bool _stylesInitialized = false;

    private int _lastScreenWidth = 0;
    private int _lastScreenHeight = 0;
    private int _windowId;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _versionInfos = AsmdefVersionManager.GetAllVersionedAsmdefs();
        AsmdefVersionManager.GetRegistryVersDict(out validVersions);
        _windowId = this.GetInstanceID();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            _isWindowVisible = !_isWindowVisible;
        }

        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            UpdateUIPositions();
        }
    }

    private void OnGUI()
    {
        InitializeStyles();

        if (showToggleButton)
        {
            if (GUI.Button(toggleButtonRect, toggleButtonText, _buttonStyle))
            {
                _isWindowVisible = !_isWindowVisible;
            }
        }

        if (!_isWindowVisible)
        {
            return;
        }

        windowRect = GUI.Window(_windowId, windowRect, DrawWindowContent, $"Assembly Versions (Toggle: {toggleKey})", _windowStyle);
    }

    /// <summary>
    /// 负责绘制窗口内部的所有内容。
    /// </summary>
    private void DrawWindowContent(int windowID)
    {
        // 允许通过点击窗口顶部来拖动
        GUI.DragWindow(new Rect(0, 0, 10000, 20));

        // --- 表头 ---
        GUILayout.BeginHorizontal();
        // 分配大约 70% 的宽度给名称，30% 给版本号
        GUILayout.Label("Assembly Name", _headerStyle, GUILayout.Width(windowRect.width * 0.65f));
        GUILayout.Label("Version", _headerStyle, GUILayout.Width(windowRect.width * 0.25f));
        GUILayout.EndHorizontal();

        // --- 分割线 ---
        GUILayout.Box("", _separatorStyle);

        // --- 内容区域 ---
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        if (_versionInfos == null || _versionInfos.Count == 0)
        {
            GUILayout.Label("未找到带版本号的程序集。", _cellStyle);
        }
        else
        {
            foreach (var info in _versionInfos)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(info.name, _cellStyle, GUILayout.Width(windowRect.width * 0.65f));
                validVersions.TryGetValue(info.name, out string version);

                if (version != null && version.Equals(info.version))
                {
                    GUILayout.Label(info.version, _cellStyle, GUILayout.Width(windowRect.width * 0.25f));
                }
                else
                {
                    // 如果版本不匹配或未在注册表中找到，则标记为红色
                    string displayText = version == null ? $"{info.version} (Not Registered)" : info.version;
                    GUILayout.Label($"<color=red>{displayText}</color>", _cellStyle, GUILayout.Width(windowRect.width * 0.25f));
                }

                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndScrollView();

        GUILayout.Box("", _separatorStyle);
        GUILayout.Label($"version: {VersionManager.Version} - {VersionManager.VersionType} - {VersionManager.BuildDate}", _cellStyle);
    }

    /// <summary>
    /// 初始化用于绘制UI的自定义样式。
    /// </summary>
    private void InitializeStyles()
    {
        if (_stylesInitialized) return;

        // --- 窗口样式 ---
        _windowStyle = new GUIStyle(GUI.skin.window);
        Texture2D backgroundTexture = CreateSolidColorTexture(new Color(0.1f, 0.1f, 0.12f, 0.95f));
        _windowStyle.normal.background = backgroundTexture;
        _windowStyle.onNormal.background = backgroundTexture;

        // --- 表头样式 ---
        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(5, 5, 5, 5)
        };

        // --- 单元格样式 ---
        _cellStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(5, 5, 2, 2)
        };

        // --- 分割线样式 ---
        _separatorStyle = new GUIStyle();
        _separatorStyle.normal.background = CreateSolidColorTexture(new Color(0.5f, 0.5f, 0.5f, 0.8f));
        _separatorStyle.stretchWidth = true;
        _separatorStyle.fixedHeight = 1;
        _separatorStyle.margin = new RectOffset(0, 0, 5, 5);

        // --- 切换按钮样式 ---
        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.normal.background = CreateSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.4f));
        _buttonStyle.hover.background = CreateSolidColorTexture(new Color(0.2f, 0.2f, 0.2f, 0.7f));
        _buttonStyle.active.background = CreateSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.9f));
        _buttonStyle.normal.textColor = Color.white;
        _buttonStyle.fontSize = 16;
        _buttonStyle.alignment = TextAnchor.MiddleCenter;

        _stylesInitialized = true;
    }

    /// <summary>
    /// 根据当前屏幕尺寸更新UI元素的位置和大小。
    /// </summary>
    private void UpdateUIPositions()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 将它们定位在屏幕右下角
        windowRect = new Rect(screenWidth - 470, screenHeight - 430, 330, 420);
        toggleButtonRect = new Rect(screenWidth - 130, screenHeight - 60, 120, 50);

        _lastScreenWidth = (int)screenWidth;
        _lastScreenHeight = (int)screenHeight;

        // Debug.Log($"UI positions updated for new resolution: {screenWidth}x{screenHeight}");
    }


    /// <summary>
    /// 创建纯色纹理。
    /// </summary>
    private static Texture2D CreateSolidColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
}