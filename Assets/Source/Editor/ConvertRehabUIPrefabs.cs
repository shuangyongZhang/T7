using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LB;
using XCharts.Runtime;

/// <summary>
/// 使用 WebUI2UnityUI 包把 Assets/Source/HTML 下的 DSL 原型烘焙为项目中的 UI Prefab。
/// 菜单：Tools -> 转换康复站UI -> 重建患者与设置界面 Prefab
/// </summary>
public static class ConvertRehabUIPrefabs
{
    private const string MainHtml = "Assets/Source/HTML/UIMainPanel.html";
    private const string PatientHtml = "Assets/Source/HTML/UIPatientInfoPanel.html";
    private const string SettingHtml = "Assets/Source/HTML/UISettingPanel.html";
    private const string AssessmentHtml = "Assets/Source/HTML/UIAssessmentPanel.html";
    private const string TrainingHtml = "Assets/Source/HTML/UITrainingPanel.html";
    private const string ReportHtml = "Assets/Source/HTML/UIReportPanel.html";
    private const string CalibrationHtml = "Assets/Source/HTML/UICalibrationPanel.html";
    private const string ExitResetHtml = "Assets/Source/HTML/UIExitResetPanel.html";
    private const string AssessmentReportTemplateHtml = "Assets/Source/HTML/UIAssessmentReportTemplate.html";
    private const string MainPrefab = "Assets/Resources/Prefabs/UI/UIMainPanel.prefab";
    private const string PatientPrefab = "Assets/Resources/Prefabs/UI/UIPatientInfoPanel.prefab";
    private const string SettingPrefab = "Assets/Resources/Prefabs/UI/UISettingPanel.prefab";
    private const string AssessmentPrefab = "Assets/Resources/Prefabs/UI/UIAssessmentPanel.prefab";
    private const string TrainingPrefab = "Assets/Resources/Prefabs/UI/UITrainingPanel.prefab";
    private const string ReportPrefab = "Assets/Resources/Prefabs/UI/UIReportPanel.prefab";
    private const string CalibrationPrefab = "Assets/Resources/Prefabs/UI/UICalibrationPanel.prefab";
    private const string ExitResetPrefab = "Assets/Resources/Prefabs/UI/UIExitResetPanel.prefab";
    private const string AssessmentReportTemplatePrefab = "Assets/Resources/Prefabs/UI/UIAssessmentReportTemplate.prefab";
    private const string CloseSpritePath = "Assets/3rdPlugins/JahroConsole/Resources/Sprites/close-sprite.png";
    private const string CompanyLogoPath = "Assets/Source/Images/APP/公共/公司logo.png";
    private const string TrainingSliderTrackPath = "Assets/Source/Images/UI/training-slider-track.png";
    private const string TrainingSliderKnobPath = "Assets/Source/Images/APP/New/bg.png";
    private const string TrainingResistanceBarPath = "Assets/Source/Images/UI/training-resistance-bar.png";
    private const string SelectionCheckmarkPath = "Assets/Source/Images/APP/确认1.png";

    [MenuItem("Tools/转换康复站UI/重建全部康复站界面 Prefab")]
    public static void RebuildAll()
    {
        BakeOne(MainHtml, MainPrefab,
            new string[]
            {
                "NavRail/NavAssessmentButton/ActiveIndicator",
                "NavRail/NavTrainingButton/ActiveIndicator",
                "NavRail/NavReportButton/ActiveIndicator",
                "NavRail/NavSettingButton/ActiveIndicator",
                "HeaderBar/BrandLogoBox/BrandLogoText"
            },
            new string[0]);

        RebuildPatientAndSetting();
        RebuildModules();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ConvertRehabUIPrefabs] 康复站全部界面 Prefab 已从 WebUI2UnityUI 原型重建完成。");
    }

    [MenuItem("Tools/转换康复站UI/重建患者与设置界面 Prefab")]
    public static void RebuildPatientAndSetting()
    {
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
                "WorkPanel/TreatmentPlanPanel",
                "WorkPanel/TreatmentPlanPanel/TreatmentPlanRowsContent/TreatmentPlanRowTemplate",
                "TreatmentPlanEditorPanel/PlanGameCard/PlanGameScroll/Viewport/Content/PlanGameRowTemplate",
                "TreatmentPlanEditorPanel",
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
        Debug.Log("[ConvertRehabUIPrefabs] 患者与设置界面 Prefab 已重建。");
    }

    [MenuItem("Tools/转换康复站UI/重建训练界面 Prefab")]
    public static void RebuildTraining()
    {
        BakeOne(TrainingHtml, TrainingPrefab,
            new string[]
            {
                "ConsolePanel/GameListCard/TrainingGameScroll/Viewport/Content/TrainingGameRowTemplate",
                "ConsolePanel/ModeCard/CpmButton",
                "ConsolePanel/ModeCard/TeachingButton",
                "ConsolePanel/DirectionCard",
                "ConsolePanel/ParameterCard"
            },
            new string[0]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ConvertRehabUIPrefabs] 训练界面 Prefab 已重建。");
    }

    [MenuItem("Tools/转换康复站UI/重建报告界面 Prefab")]
    public static void RebuildReport()
    {
        BakeOne(ReportHtml, ReportPrefab,
            new string[]
            {
                "ConsolePanel/ReportRowsContent/ReportRowTemplate",
                "ConsolePanel/ReportPager/PageNumberTemplate",
                "ConsolePanel/ReportPager/PageEllipsisTemplate",
                "ConsolePanel/EmptyListState",
                "CanvasPanel/DetailCard/AssessmentScroll",
                "CanvasPanel/DetailCard/TrainingDetailPanel",
                "CanvasPanel/HistoryOverviewPanel/ComparisonViewPanel"
            },
            new string[0]);

        BakeOne(AssessmentReportTemplateHtml, AssessmentReportTemplatePrefab,
            new string[0],
            new string[0]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ConvertRehabUIPrefabs] 报告界面 Prefab 已重建。");
    }

    [MenuItem("Tools/转换康复站UI/重建评估训练报告 Prefab")]
    public static void RebuildModules()
    {
        BakeOne(AssessmentHtml, AssessmentPrefab,
            new string[] { "CanvasPanel/MeasureCard/GaugeArea/GaugeTickTemplate" },
            new string[0]);

        RebuildTraining();

        BakeOne(ReportHtml, ReportPrefab,
            new string[]
            {
                "ConsolePanel/ReportRowsContent/ReportRowTemplate",
                "ConsolePanel/ReportPager/PageNumberTemplate",
                "ConsolePanel/ReportPager/PageEllipsisTemplate",
                "ConsolePanel/EmptyListState",
                "CanvasPanel/DetailCard/AssessmentScroll",
                "CanvasPanel/DetailCard/TrainingDetailPanel",
                "CanvasPanel/HistoryOverviewPanel/ComparisonViewPanel"
            },
            new string[0]);

        BakeOne(AssessmentReportTemplateHtml, AssessmentReportTemplatePrefab,
            new string[0],
            new string[0]);

        BakeOne(CalibrationHtml, CalibrationPrefab, new string[0], new string[0]);
        BakeOne(ExitResetHtml, ExitResetPrefab, new string[0], new string[0]);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ConvertRehabUIPrefabs] 评估、训练、报告界面 Prefab 已重建。");
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

        if (prefabPath == MainPrefab) ApplyCompanyLogo(root);

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

        if (prefabPath == TrainingPrefab) StyleTrainingControls(root);
        ApplySelectionCheckmarks(root);

        string rowTemplatePath = null;
        if (prefabPath == PatientPrefab) rowTemplatePath = "CanvasPanel/PatientRowsContent/PatientRowTemplate";
        else if (prefabPath == SettingPrefab) rowTemplatePath = "WorkPanel/DoctorPanel/DoctorRowsContent/DoctorRowTemplate";
        else if (prefabPath == ReportPrefab) rowTemplatePath = "ConsolePanel/ReportRowsContent/ReportRowTemplate";
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

        if (prefabPath == AssessmentReportTemplatePrefab) AttachReportCharts(root);
        if (prefabPath == ReportPrefab) AttachOverviewCharts(root);
        if (prefabPath == CalibrationPrefab) AddResetDiagram(root, "CalibrationInstructionCard/CalibrationDiagram");
        if (prefabPath == ExitResetPrefab) AddResetPhoto(root, "ResetPhoto");

        RestoreInputConstraints(root, prefabPath);

        foreach (string path in inactivePaths)
        {
            Transform node = root.transform.Find(path);
            if (node != null) node.gameObject.SetActive(false);
            else Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到需隐藏节点: " + path);
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        UnityEngine.Object.DestroyImmediate(canvasGo);
        Debug.Log("[ConvertRehabUIPrefabs] 已写入 Prefab: " + prefabPath);
    }

    private static void ApplySelectionCheckmarks(GameObject root)
    {
        Sprite checkmark = AssetDatabase.LoadAssetAtPath<Sprite>(SelectionCheckmarkPath);
        if (checkmark == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到选择勾选图: " + SelectionCheckmarkPath);
            return;
        }

        foreach (TMP_Dropdown dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            Transform node = dropdown.transform.Find("Template/Viewport/Content/Item/Item Checkmark");
            Image image = node != null ? node.GetComponent<Image>() : null;
            if (image != null) SetSelectionCheckmark(image, checkmark);
        }

        foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>(true))
        {
            Image image = toggle.graphic as Image;
            if (image == null || image.sprite != null) continue;
            if (image.color.r > 0.05f || image.color.g > 0.05f || image.color.b > 0.05f) continue;
            SetSelectionCheckmark(image, checkmark);
        }
    }

    private static void SetSelectionCheckmark(Image image, Sprite checkmark)
    {
        image.sprite = checkmark;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static void StyleTrainingControls(GameObject root)
    {
        Sprite sliderTrack = AssetDatabase.LoadAssetAtPath<Sprite>(TrainingSliderTrackPath);
        Sprite sliderKnob = AssetDatabase.LoadAssetAtPath<Sprite>(TrainingSliderKnobPath);
        Sprite resistanceBar = AssetDatabase.LoadAssetAtPath<Sprite>(TrainingResistanceBarPath);

        Transform resistanceNode = root.transform.Find("ConsolePanel/ParameterCard/ResistanceSlider");
        if (resistanceNode != null)
        {
            Slider slider = resistanceNode.GetComponent<Slider>();
            StyleTrainingSlider(slider, resistanceBar, sliderKnob, Color.white, true);
            slider.minValue = 1f;
            slider.maxValue = 10f;
            slider.wholeNumbers = true;
            slider.value = 3f;
        }

        Transform durationNode = root.transform.Find("ConsolePanel/ParameterCard/DurationSlider");
        if (durationNode != null)
        {
            Slider slider = durationNode.GetComponent<Slider>();
            StyleTrainingSlider(slider, sliderTrack, sliderKnob, new Color(0.82f, 0.835f, 0.86f, 1f), false);
            slider.minValue = 5f;
            slider.maxValue = 30f;
            slider.wholeNumbers = true;
            slider.value = 20f;
        }

        Transform toggleNode = root.transform.Find("ConsolePanel/DirectionCard/DirectionLimitToggle");
        if (toggleNode == null) return;
        Toggle toggle = toggleNode.GetComponent<Toggle>();
        Transform labelNode = toggleNode.Find("Label");
        if (labelNode != null) UnityEngine.Object.DestroyImmediate(labelNode.gameObject);
        Transform trackNode = toggleNode.Find("Background");
        if (toggle == null || trackNode == null) return;
        Image track = trackNode.GetComponent<Image>();
        Transform knobNode = trackNode.Find("Checkmark");
        if (knobNode == null && toggle.graphic != null) knobNode = toggle.graphic.transform;
        if (track == null || knobNode == null) return;

        track.sprite = sliderTrack;
        track.color = new Color(0.82f, 0.835f, 0.86f, 1f);
        track.raycastTarget = true;
        RectTransform trackRect = track.rectTransform;
        trackRect.anchorMin = Vector2.zero;
        trackRect.anchorMax = Vector2.one;
        trackRect.offsetMin = Vector2.zero;
        trackRect.offsetMax = Vector2.zero;

        knobNode.name = "Knob";
        Image knob = knobNode.GetComponent<Image>();
        knob.sprite = sliderKnob;
        knob.color = Color.white;
        knob.raycastTarget = false;
        RectTransform knobRect = knob.rectTransform;
        knobRect.anchorMin = new Vector2(0.5f, 0.5f);
        knobRect.anchorMax = new Vector2(0.5f, 0.5f);
        knobRect.sizeDelta = new Vector2(12f, 12f);
        knobRect.anchoredPosition = new Vector2(-12f, 0f);

        toggle.transition = Selectable.Transition.None;
        toggle.targetGraphic = track;
        toggle.graphic = null;
        toggle.isOn = false;
    }

    private static void StyleTrainingSlider(Slider slider, Sprite trackSprite, Sprite handleSprite, Color trackColor, bool hideFill)
    {
        if (slider == null) return;
        slider.transition = Selectable.Transition.None;
        Transform backgroundNode = slider.transform.Find("Background");
        Transform fillNode = slider.transform.Find("Fill Area/Fill");
        Image background = backgroundNode != null ? backgroundNode.GetComponent<Image>() : null;
        Image fill = fillNode != null ? fillNode.GetComponent<Image>() : null;
        Image handle = slider.handleRect != null ? slider.handleRect.GetComponent<Image>() : null;

        if (background != null)
        {
            background.sprite = trackSprite;
            background.color = trackColor;
            background.raycastTarget = true;
            slider.targetGraphic = background;
        }
        if (fill != null)
        {
            fill.sprite = trackSprite;
            fill.color = trackColor;
            fill.raycastTarget = false;
            if (hideFill) fill.gameObject.SetActive(false);
        }
        if (handle != null)
        {
            handle.sprite = handleSprite;
            handle.color = Color.white;
            handle.raycastTarget = true;
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(handleRect.anchorMin.x, 0.5f);
            handleRect.anchorMax = new Vector2(handleRect.anchorMax.x, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            handleRect.sizeDelta = new Vector2(24f, 24f);
        }
        Transform slideArea = slider.transform.Find("Handle Slide Area");
        if (slideArea != null)
        {
            RectTransform slideRect = slideArea.GetComponent<RectTransform>();
            slideRect.anchorMin = new Vector2(0f, 0.5f);
            slideRect.anchorMax = new Vector2(1f, 0.5f);
            slideRect.anchoredPosition = Vector2.zero;
            slideRect.sizeDelta = new Vector2(-24f, 0f);
        }
    }

    private static void AttachReportCharts(GameObject root)
    {
        const string content = "ReportContent/";
        AddRomRangeChart(root, content + "RomCardA/RomRangeChart");
        AddRomRangeChart(root, content + "RomCardB/RomRangeChart");
        AddRomRangeChart(root, content + "RomCardC/RomRangeChart");
        AddRomRangeChart(root, content + "RomCardD/RomRangeChart");
        AddReportChart(root, content + "StrengthRadarCard/StrengthRadarChart", ReportChartGraphic.ChartKind.StrengthRadar);
        AddReportChart(root, content + "BaselineCard/BaselineChart", ReportChartGraphic.ChartKind.BaselineLine);
    }

    private static void AddRomRangeChart(GameObject root, string path)
    {
        Transform node = root.transform.Find(path);
        if (node == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到 ROM 三维图表节点: " + path);
            return;
        }
        Image image = node.GetComponent<Image>();
        if (image != null) UnityEngine.Object.DestroyImmediate(image);
        ReportChartGraphic oldChart = node.GetComponent<ReportChartGraphic>();
        if (oldChart != null) UnityEngine.Object.DestroyImmediate(oldChart);
        RomRangeChart chart = node.GetComponent<RomRangeChart>();
        if (chart == null) chart = node.gameObject.AddComponent<RomRangeChart>();
        chart.raycastTarget = false;
    }

    private static void AttachOverviewCharts(GameObject root)
    {
        AddOverviewPieChart(root, "CanvasPanel/HistoryOverviewPanel/SummaryViewPanel/SummaryPieCard/SummaryPieChart");
        AddOverviewLineChart(root, "CanvasPanel/HistoryOverviewPanel/ComparisonViewPanel/ComparisonLineCard/ComparisonLineChart");
    }

    private static void AddOverviewPieChart(GameObject root, string path)
    {
        Transform node = root.transform.Find(path);
        if (node == null) return;
        Image image = node.GetComponent<Image>();
        if (image != null) UnityEngine.Object.DestroyImmediate(image);
        PieChart chart = node.GetComponent<PieChart>();
        if (chart == null) chart = node.gameObject.AddComponent<PieChart>();
        chart.Init();
        chart.raycastTarget = false;
    }

    private static void AddOverviewLineChart(GameObject root, string path)
    {
        Transform node = root.transform.Find(path);
        if (node == null) return;
        Image image = node.GetComponent<Image>();
        if (image != null) UnityEngine.Object.DestroyImmediate(image);
        LineChart chart = node.GetComponent<LineChart>();
        if (chart == null) chart = node.gameObject.AddComponent<LineChart>();
        chart.Init();
        chart.raycastTarget = false;
    }

    private static void AddResetDiagram(GameObject root, string path)
    {
        Transform node = root.transform.Find(path);
        if (node == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到复位示意图节点: " + path);
            return;
        }
        Image image = node.GetComponent<Image>();
        if (image != null) UnityEngine.Object.DestroyImmediate(image);
        UIResetDiagram diagram = node.GetComponent<UIResetDiagram>();
        if (diagram == null) diagram = node.gameObject.AddComponent<UIResetDiagram>();
        diagram.raycastTarget = false;
        diagram.color = Color.white;
    }

    private static void AddResetPhoto(GameObject root, string path)
    {
        Transform node = root.transform.Find(path);
        if (node == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到复位照片节点: " + path);
            return;
        }
        Image image = node.GetComponent<Image>();
        if (image == null) image = node.gameObject.AddComponent<Image>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Images/ExitResetReference.jpg");
        if (sprite == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到复位照片资源: Assets/Resources/Images/ExitResetReference.jpg");
            return;
        }
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static void ApplyCompanyLogo(GameObject root)
    {
        Transform logo = root.transform.Find("HeaderBar/BrandLogoBox");
        if (logo == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到公司 Logo 节点: HeaderBar/BrandLogoBox");
            return;
        }

        Image image = logo.GetComponent<Image>();
        if (image == null) image = logo.gameObject.AddComponent<Image>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CompanyLogoPath);
        if (sprite == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到公司 Logo 资源: " + CompanyLogoPath);
            return;
        }

        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
    }

    private static void AddReportChart(GameObject root, string path, ReportChartGraphic.ChartKind kind)
    {
        Transform node = root.transform.Find(path);
        if (node == null)
        {
            Debug.LogWarning("[ConvertRehabUIPrefabs] 未找到报告图表节点: " + path);
            return;
        }
        Image image = node.GetComponent<Image>();
        if (image != null) UnityEngine.Object.DestroyImmediate(image);
        ReportChartGraphic chart = node.GetComponent<ReportChartGraphic>();
        if (chart == null) chart = node.gameObject.AddComponent<ReportChartGraphic>();
        chart.Kind = kind;
        chart.color = Color.white;
        chart.raycastTarget = false;
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
