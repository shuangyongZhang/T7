using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 使用 WebUI2UnityUI 包把 Assets/Source/HTML 下的 DSL 原型烘焙为项目中的 UI Prefab。
/// 菜单：Tools -> 转换康复站UI -> 重建患者与设置界面 Prefab
/// </summary>
public static class ConvertRehabUIPrefabs
{
    private const string MainHtml = "Assets/Source/HTML/UIMainPanel.html";
    private const string PatientHtml = "Assets/Source/HTML/UIPatientInfoPanel.html";
    private const string SettingHtml = "Assets/Source/HTML/UISettingPanel.html";
    private const string MainPrefab = "Assets/Resources/Prefabs/UI/UIMainPanel.prefab";
    private const string PatientPrefab = "Assets/Resources/Prefabs/UI/UIPatientInfoPanel.prefab";
    private const string SettingPrefab = "Assets/Resources/Prefabs/UI/UISettingPanel.prefab";
    private const string CloseSpritePath = "Assets/3rdPlugins/JahroConsole/Resources/Sprites/close-sprite.png";

    [MenuItem("Tools/转换康复站UI/重建患者与设置界面 Prefab")]
    public static void RebuildAll()
    {
        BakeOne(MainHtml, MainPrefab,
            new string[]
            {
                "NavRail/NavAssessmentButton/ActiveIndicator",
                "NavRail/NavTrainingButton/ActiveIndicator",
                "NavRail/NavReportButton/ActiveIndicator",
                "NavRail/NavSettingButton/ActiveIndicator"
            },
            new string[0]);

        BakeOne(PatientHtml, PatientPrefab,
            new string[]
            {
                "CanvasPanel/PatientRowsContent/PatientRowTemplate",
                "CanvasPanel/PatientPager/PageNumberTemplate",
                "CanvasPanel/PatientPager/PageEllipsisTemplate",
                "AddPatientPanel",
                "EditPatientPanel"
            },
            new string[0]);

        BakeOne(SettingHtml, SettingPrefab,
            new string[]
            {
                "WorkPanel/DoctorPanel/DoctorRowsContent/DoctorRowTemplate",
                "WorkPanel/DoctorPanel/DoctorPager/PageNumberTemplate",
                "WorkPanel/DoctorPanel/DoctorPager/PageEllipsisTemplate",
                "DoctorAddPanel",
                "DoctorEditPanel",
                "WorkPanel/HospitalPanel",
                "WorkPanel/DoctorPanel",
                "MenuPanel/DoctorButton/LockImage",
                "MenuPanel/HospitalButton/LockImage"
            },
            new string[]
            {
                "WorkPanel/HospitalPanel/HospitalLogo/Logo",
                "WorkPanel/HospitalPanel/HospitalLogo/Logo/Spare"
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ConvertRehabUIPrefabs] 患者与设置界面 Prefab 已从 WebUI2UnityUI 原型重建完成。");
    }

    private static void BakeOne(string htmlPath, string prefabPath, string[] inactivePaths, string[] rawImagePaths)
    {
        TextAsset html = AssetDatabase.LoadAssetAtPath<TextAsset>(htmlPath);
        if (html == null)
        {
            throw new FileNotFoundException("找不到 UI 原型 HTML: " + htmlPath);
        }

        Type editorType = Type.GetType("WebUI2UnityUIEditor, WebUI2UnityUI.Editor");
        if (editorType == null)
        {
            throw new TypeLoadException("找不到 WebUI2UnityUIEditor，请确认 com.shmqc.webui2unityui 包已安装并编译。");
        }

        GameObject canvasGo = new GameObject("TEMP_WebUI2UnityUI_Canvas");
        canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        EditorWindow window = EditorWindow.GetWindow(editorType, false, "WebUI转UnityUI工具", false);
        SetPrivateField(window, editorType, "htmlAsset", html);
        SetPrivateField(window, editorType, "targetCanvas", canvasGo.GetComponent<Canvas>());
        SetPrivateField(window, editorType, "rawHtmlSource", "");
        SetPrivateField(window, editorType, "overrideFont", null);

        MethodInfo bakeMethod = editorType.GetMethod("ExecuteBake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (bakeMethod == null)
        {
            throw new MissingMethodException(editorType.FullName, "ExecuteBake");
        }
        bakeMethod.Invoke(window, null);
        window.Close();

        string rootName = Path.GetFileNameWithoutExtension(prefabPath);
        GameObject root = null;
        foreach (Transform child in canvasGo.transform)
        {
            if (child.name == rootName)
            {
                root = child.gameObject;
                break;
            }
        }
        if (root == null)
        {
            UnityEngine.Object.DestroyImmediate(canvasGo);
            throw new InvalidOperationException("烘焙未生成预期根节点: " + rootName);
        }

        foreach (string path in inactivePaths)
        {
            Transform node = root.transform.Find(path);
            if (node != null) node.gameObject.SetActive(false);
            else Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到需隐藏节点: " + path);
        }

        foreach (string path in rawImagePaths)
        {
            Transform node = root.transform.Find(path);
            if (node == null)
            {
                Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到 RawImage 节点: " + path);
                continue;
            }
            Image image = node.GetComponent<Image>();
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
            if (node.GetComponent<RawImage>() == null)
            {
                RawImage rawImage = node.gameObject.AddComponent<RawImage>();
                rawImage.color = Color.white;
            }
        }

        // 让 Slider 的填充条与背景条左对齐、同高，避免前景/背景错位
        foreach (Slider slider in root.GetComponentsInChildren<Slider>(true))
        {
            Transform fillArea = slider.transform.Find("Fill Area");
            if (fillArea == null) continue;
            RectTransform fillRect = fillArea.GetComponent<RectTransform>();
            if (fillRect == null) continue;
            fillRect.anchorMin = new Vector2(0f, 0.25f);
            fillRect.anchorMax = new Vector2(1f, 0.75f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        // 关闭按钮改用 close-sprite 图片（SIMHEI 字体缺 ✕ 字形，文字会渲染成空框）
        Sprite closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CloseSpritePath);
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (!button.name.EndsWith("CloseButton")) continue;
            Image icon = button.GetComponent<Image>();
            if (icon == null || closeSprite == null) continue;
            icon.sprite = closeSprite;
            icon.preserveAspect = true;
            icon.color = new Color(0.557f, 0.557f, 0.576f, 1f);
            icon.raycastTarget = true;
        }

        // 下拉箭头：字体无三角字形，用两条旋转细条拼出 chevron
        foreach (TMP_Dropdown dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            Transform arrow = dropdown.transform.Find("Arrow");
            if (arrow == null) continue;
            Image arrowImage = arrow.GetComponent<Image>();
            if (arrowImage != null) UnityEngine.Object.DestroyImmediate(arrowImage);
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(12, 12);
            arrowRect.anchoredPosition = new Vector2(-14, 0);
            for (int i = 0; i < 2; i++)
            {
                GameObject stroke = new GameObject(i == 0 ? "ChevronLeft" : "ChevronRight");
                stroke.transform.SetParent(arrow, false);
                RectTransform strokeRect = stroke.AddComponent<RectTransform>();
                strokeRect.sizeDelta = new Vector2(2, 8);
                strokeRect.anchoredPosition = new Vector2(i == 0 ? -2f : 2f, 0f);
                strokeRect.localEulerAngles = new Vector3(0f, 0f, i == 0 ? 45f : -45f);
                Image strokeImage = stroke.AddComponent<Image>();
                strokeImage.color = new Color(0.271f, 0.318f, 0.369f, 1f);
                strokeImage.raycastTarget = false;
            }
        }

        // 滑条拇指 16×16（运行时 Slider 会把 handle 的 x 锚点收拢，sizeDelta.x 即实际宽度）
        foreach (Slider slider in root.GetComponentsInChildren<Slider>(true))
        {
            if (slider.handleRect != null) slider.handleRect.sizeDelta = new Vector2(16, 8);
        }

        string rowTemplatePath = null;
        if (prefabPath == PatientPrefab) rowTemplatePath = "CanvasPanel/PatientRowsContent/PatientRowTemplate";
        else if (prefabPath == SettingPrefab) rowTemplatePath = "WorkPanel/DoctorPanel/DoctorRowsContent/DoctorRowTemplate";
        if (!string.IsNullOrEmpty(rowTemplatePath))
        {
            Transform rowNode = root.transform.Find(rowTemplatePath);
            if (rowNode != null)
            {
                Button rowButton = rowNode.GetComponent<Button>();
                if (rowButton == null) rowButton = rowNode.gameObject.AddComponent<Button>();
                rowButton.targetGraphic = rowNode.GetComponent<Image>();
            }
        }

        // NavRail 垂直自动布局：按钮隐藏/新增时自动紧凑排列；右缘竖分隔线不参与布局
        if (prefabPath == MainPrefab)
        {
            Transform navRail = root.transform.Find("NavRail");
            if (navRail != null)
            {
                VerticalLayoutGroup layout = navRail.GetComponent<VerticalLayoutGroup>();
                if (layout == null) layout = navRail.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(8, 8, 16, 16);
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                Transform divider = navRail.Find("NavRailDivider");
                if (divider != null)
                {
                    LayoutElement layoutElement = divider.GetComponent<LayoutElement>();
                    if (layoutElement == null) layoutElement = divider.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;
                }
            }
            else Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到 NavRail 节点，未应用垂直布局");
        }

        RestoreInputConstraints(root, prefabPath);

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        UnityEngine.Object.DestroyImmediate(canvasGo);
        Debug.Log("[ConvertRehabUIPrefabs] 已写入 Prefab: " + prefabPath);
    }

    // WebUI2UnityUI 烘焙出的输入框全是无约束 Standard，这里按旧版 prefab 的约束逐字段恢复，
    // 保证重样式不改变输入校验功能。科室旧版带纯数字正则，与新界面文本 placeholder 冲突，只恢复字数上限。
    private static void RestoreInputConstraints(GameObject root, string prefabPath)
    {
        if (prefabPath == PatientPrefab)
        {
            foreach (string prefix in new[] { "AddPatientPanel/Add", "EditPatientPanel/Edit" })
            {
                SetInput(root, prefix + "CardNumInputField", TMP_InputField.ContentType.IntegerNumber, 18);
                SetInput(root, prefix + "NameInputField", TMP_InputField.ContentType.Standard, 16);
                SetNumericInput(root, prefix + "AgeInputField");
                SetNumericInput(root, prefix + "StatureInputField");
                SetNumericInput(root, prefix + "WeightInputField");
                SetInput(root, prefix + "DiagnoseInputField", TMP_InputField.ContentType.Standard, 150);
            }
            TMP_InputField createTime = FindInput(root, "EditPatientPanel/EditCreateTimeInputField");
            if (createTime != null) createTime.interactable = false;
        }
        else if (prefabPath == SettingPrefab)
        {
            SetPasswordInput(root, "DoctorAddPanel/AddPassword1InputField");
            SetPasswordInput(root, "DoctorAddPanel/AddPassword2InputField");
            SetPasswordInput(root, "DoctorEditPanel/InfoPassword1InputField");
            SetPasswordInput(root, "DoctorEditPanel/InfoPassword2InputField");
            foreach (string prefix in new[] { "DoctorAddPanel/Add", "DoctorEditPanel/Info" })
            {
                SetInput(root, prefix + "AccountInputField", TMP_InputField.ContentType.Standard, 16);
                SetInput(root, prefix + "NameInputField", TMP_InputField.ContentType.Standard, 16);
                SetInput(root, prefix + "TeleInputField", TMP_InputField.ContentType.Alphanumeric, 11);
                SetInput(root, prefix + "DeptInputField", TMP_InputField.ContentType.Standard, 16);
            }
        }
    }

    private static TMP_InputField FindInput(GameObject root, string path)
    {
        Transform node = root.transform.Find(path);
        if (node == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到输入框节点: " + path);
            return null;
        }
        return node.GetComponent<TMP_InputField>();
    }

    private static void SetInput(GameObject root, string path, TMP_InputField.ContentType type, int limit)
    {
        TMP_InputField input = FindInput(root, path);
        if (input == null) return;
        input.contentType = type;
        input.characterLimit = limit;
    }

    private static void SetNumericInput(GameObject root, string path)
    {
        TMP_InputField input = FindInput(root, path);
        if (input == null) return;
        input.contentType = TMP_InputField.ContentType.Custom;
        input.characterValidation = TMP_InputField.CharacterValidation.Regex;
        SetRegexValue(input, "^[0-9]+$");
        input.characterLimit = 3;
    }

    private static void SetPasswordInput(GameObject root, string path)
    {
        TMP_InputField input = FindInput(root, path);
        if (input == null) return;
        input.contentType = TMP_InputField.ContentType.Custom;
        input.inputType = TMP_InputField.InputType.Password;
        input.asteriskChar = '*';
        input.characterValidation = TMP_InputField.CharacterValidation.Regex;
        SetRegexValue(input, @"[^~\u4E00-\u9FA5]+");
        input.characterLimit = 16;
    }

    // TMP 3.0.9 未公开 regexValue 属性，只能经序列化字段写入
    private static void SetRegexValue(TMP_InputField input, string regex)
    {
        SerializedObject so = new SerializedObject(input);
        so.FindProperty("m_RegexValue").stringValue = regex;
        so.ApplyModifiedProperties();
    }

    private static void SetPrivateField(object target, Type type, string fieldName, object value)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
        {
            throw new MissingFieldException(type.FullName, fieldName);
        }
        field.SetValue(target, value);
    }
}
