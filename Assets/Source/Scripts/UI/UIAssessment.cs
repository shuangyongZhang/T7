using System;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    [UIResource("UIAssessmentPanel")]
    public class UIAssessment : UILogicResident
    {
        [InjectPath("ConsolePanel/JointCard/ShoulderButton")] private Button shoulderButton;
        [InjectPath("ConsolePanel/JointCard/ElbowButton")] private Button elbowButton;
        [InjectPath("ConsolePanel/JointCard/ForearmButton")] private Button forearmButton;
        [InjectPath("ConsolePanel/ModeCard/AromButton")] private Button aromButton;
        [InjectPath("ConsolePanel/ModeCard/PromButton")] private Button promButton;
        [InjectPath("ConsolePanel/AxisCard/FlexionAxisButton")] private Button flexionAxisButton;
        [InjectPath("ConsolePanel/AxisCard/AbductionAxisButton")] private Button abductionAxisButton;
        [InjectPath("ConsolePanel/AxisCard/HorizontalAxisButton")] private Button horizontalAxisButton;
        [InjectPath("ConsolePanel/AxisCard/RotationAxisButton")] private Button rotationAxisButton;
        [InjectPath("ConsolePanel/ActionCard/StartButton")] private Button startButton;
        [InjectPath("ConsolePanel/ActionCard/SaveButton")] private Button saveButton;
        [InjectPath("ConsolePanel/SessionCard/MeasurementStateText")] private TMP_Text measurementStateText;
        [InjectPath("ConsolePanel/SessionCard/ElapsedText")] private TMP_Text elapsedText;
        [InjectPath("ConsolePanel/SessionCard/SampleCountText")] private TMP_Text sampleCountText;
        [InjectPath("ConsolePanel/SessionCard/DeviceStateText")] private TMP_Text deviceStateText;
        [InjectPath("CanvasPanel/MeasureCard/MeasureTitleText")] private TMP_Text measureTitleText;
        [InjectPath("CanvasPanel/MeasureCard/CurrentMetric/CurrentAngleText")] private TMP_Text currentAngleText;
        [InjectPath("CanvasPanel/MeasureCard/MaxMetric/MaxAngleText")] private TMP_Text maxAngleText;
        [InjectPath("CanvasPanel/MeasureCard/MinMetric/MinAngleText")] private TMP_Text minAngleText;
        [InjectPath("CanvasPanel/MeasureCard/GaugeArea/GaugeTicks")] private RectTransform gaugeTicks;
        [InjectPath("CanvasPanel/MeasureCard/GaugeArea/GaugeTickTemplate")] private GameObject gaugeTickTemplate;
        [InjectPath("CanvasPanel/MeasureCard/GaugeArea/GaugePointer")] private RectTransform gaugePointer;
        [InjectPath("CanvasPanel/MeasureCard/ResultCard/ResultPatientText")] private TMP_Text resultPatientText;
        [InjectPath("CanvasPanel/MeasureCard/ResultCard/ResultSelectionText")] private TMP_Text resultSelectionText;
        [InjectPath("CanvasPanel/MeasureCard/ResultCard/ResultRangeText")] private TMP_Text resultRangeText;
        [InjectPath("CanvasPanel/MeasureCard/ResultCard/ArchiveStateText")] private TMP_Text archiveStateText;

        private static readonly Color SelectedBackground = new Color(0.937f, 0.965f, 1f, 1f);
        private static readonly Color NormalBackground = Color.white;
        private static readonly Color SelectedText = new Color(0.078f, 0.337f, 0.941f, 1f);
        private static readonly Color NormalText = new Color(0.271f, 0.318f, 0.369f, 1f);

        private AssessmentJoint joint = AssessmentJoint.Shoulder;
        private string axis = "flexion";
        private bool arom = true;
        private bool measuring;
        private bool hasSamples;
        private bool archived;
        private float currentAngle;
        private float minAngle;
        private float maxAngle;
        private float elapsed;
        private int sampleCount;
        private DateTime sessionStart;
        private float refreshAccumulator;
        private RehabSessionRecorder recorder = new RehabSessionRecorder();
        private bool completeCalibration;
        private Action afterCalibration;

        public override void OnInit()
        {
            base.OnInit();
            RehabTelemetryHub.ConnectionChanged += OnConnectionChanged;
            RehabTelemetryHub.SampleReceived += OnSampleReceived;
            RehabTelemetryHub.MetricsReceived += OnMetricsReceived;

            shoulderButton.onClick.AddListener(() => SelectJoint(AssessmentJoint.Shoulder));
            elbowButton.onClick.AddListener(() => SelectJoint(AssessmentJoint.Elbow));
            forearmButton.onClick.AddListener(() => SelectJoint(AssessmentJoint.Forearm));
            aromButton.onClick.AddListener(() => SelectMode(true));
            promButton.onClick.AddListener(() => SelectMode(false));
            flexionAxisButton.onClick.AddListener(() => SelectAxis("flexion"));
            abductionAxisButton.onClick.AddListener(() => SelectAxis("abduction"));
            horizontalAxisButton.onClick.AddListener(() => SelectAxis("horizontal"));
            rotationAxisButton.onClick.AddListener(() => SelectAxis("rotation"));
            startButton.onClick.AddListener(ToggleMeasuring);
            saveButton.onClick.AddListener(SaveRecord);
            startButton.interactable = RehabTelemetryHub.IsConnected;

            CreateGaugeTicks();
            SelectJoint(AssessmentJoint.Shoulder);
            SelectMode(true);
            SelectAxis("flexion");
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            completeCalibration = args != null && args.Length > 0 && args[0] is bool && (bool)args[0];
            afterCalibration = args != null && args.Length > 1 ? args[1] as Action : null;
            ResetMeasurement();
            RefreshUI();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (measuring)
            {
                elapsed += Time.deltaTime;
            }

            refreshAccumulator += Time.deltaTime;
            if (refreshAccumulator >= 0.1f)
            {
                refreshAccumulator = 0f;
                RefreshUI();
            }
        }

        private void SelectJoint(AssessmentJoint value)
        {
            joint = value;
            SetSelected(shoulderButton, joint == AssessmentJoint.Shoulder);
            SetSelected(elbowButton, joint == AssessmentJoint.Elbow);
            SetSelected(forearmButton, joint == AssessmentJoint.Forearm);
            ResetMeasurement();
            RefreshUI();
        }

        private void SelectMode(bool value)
        {
            arom = value;
            SetSelected(aromButton, arom);
            SetSelected(promButton, !arom);
            ResetMeasurement();
            RefreshUI();
        }

        private void SelectAxis(string value)
        {
            axis = value;
            SetSelected(flexionAxisButton, axis == "flexion");
            SetSelected(abductionAxisButton, axis == "abduction");
            SetSelected(horizontalAxisButton, axis == "horizontal");
            SetSelected(rotationAxisButton, axis == "rotation");
            ResetMeasurement();
            RefreshUI();
        }

        private void ResetMeasurement()
        {
            measuring = false;
            hasSamples = false;
            archived = false;
            recorder.Reset();
            sampleCount = 0;
            elapsed = 0f;
            minAngle = 0f;
            maxAngle = 0f;
            sessionStart = DateTime.MinValue;
            currentAngle = 0f;
            SetButtonText(startButton, "开始动态角度采集");
            startButton.GetComponent<Image>().color = new Color(0.094f, 0.118f, 0.145f, 1f);
            startButton.interactable = RehabTelemetryHub.IsConnected;
            saveButton.interactable = false;
        }

        private void ToggleMeasuring()
        {
            if (Global.CurPatientInfo == null)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("请先选择患者");
                return;
            }

            if (!RehabTelemetryHub.IsConnected)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("机械臂通讯协议未接入，暂时无法采集");
                return;
            }

            if (!measuring && archived) ResetMeasurement();
            measuring = !measuring;
            if (measuring)
            {
                if (sessionStart == DateTime.MinValue) sessionStart = DateTime.Now;
                SetButtonText(startButton, "结束采集");
            }
            else
            {
                SetButtonText(startButton, "继续采集");
                saveButton.interactable = hasSamples;
            }
            RefreshUI();
        }

        private void OnConnectionChanged(bool connected)
        {
            startButton.interactable = connected;
            if (!connected && measuring)
            {
                measuring = false;
                SetButtonText(startButton, "继续采集");
                saveButton.interactable = hasSamples;
            }
            RefreshUI();
        }

        private void OnSampleReceived(RehabSample sample)
        {
            if (!measuring || !recorder.AddSample(sample)) return;
            sampleCount = recorder.SampleCount;
            hasSamples = true;
            currentAngle = sample.CurrentAngleDegrees;
            minAngle = sampleCount == 1 ? currentAngle : Mathf.Min(minAngle, currentAngle);
            maxAngle = sampleCount == 1 ? currentAngle : Mathf.Max(maxAngle, currentAngle);
            RefreshUI();
        }

        private void OnMetricsReceived(RehabStrengthMetrics strength, RehabBaselineMetrics baseline)
        {
            if (!measuring) return;
            recorder.Strength = strength;
            recorder.Baseline = baseline;
        }

        private void SaveRecord()
        {
            if (Global.CurPatientInfo == null || !hasSamples)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("请先完成一次动态角度采集");
                return;
            }

            measuring = false;
            DateTime now = DateTime.Now;
            AssessmentReportPayload payload = new AssessmentReportPayload
            {
                Mode = arom ? "AROM" : "PROM",
                Axis = GetAxisName(),
                CurrentAngle = currentAngle,
                MinAngle = minAngle,
                MaxAngle = maxAngle,
                Rom = recorder.CalculateRom(),
                Strength = recorder.Strength,
                Baseline = recorder.Baseline
            };
            RehabReport report = new RehabReport
            {
                Title = GetJointName() + " ROM 评估报告",
                PatientID = Global.CurPatientInfo.ID,
                Kind = RehabReportKind.Assessment,
                Joint = joint,
                PayloadJson = JsonUtility.ToJson(payload),
                StartTime = sessionStart == DateTime.MinValue ? now : sessionStart,
                EndTime = now,
                CreateTime = now
            };

            if (!DBManagerModule.Instance.InsertRehabReport(report, recorder.Samples))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "评估报告写入数据库失败", "确定");
                return;
            }

            archived = true;
            SetButtonText(startButton, "重新开始采集");
            startButton.GetComponent<Image>().color = new Color(0.094f, 0.118f, 0.145f, 1f);
            startButton.interactable = RehabTelemetryHub.IsConnected;
            saveButton.interactable = false;
            RefreshUI();
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("评估极值已归档，可在报告列表查看");

            if (completeCalibration)
            {
                completeCalibration = false;
                if (!CalibrationWorkflow.MarkCalibrated(Global.CurPatientInfo))
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("错误", "评估已归档，但患者标定状态写入失败", "确定");
                    return;
                }
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("标定流程已完成");
                Action next = afterCalibration;
                afterCalibration = null;
                if (next != null) next();
            }
        }

        private void RefreshUI()
        {
            deviceStateText.text = RehabTelemetryHub.IsConnected ? "设备状态：真实数据已连接" : "设备状态：通讯未接入";
            deviceStateText.color = RehabTelemetryHub.IsConnected ? new Color(0.086f, 0.639f, 0.29f, 1f) : new Color(0.557f, 0.557f, 0.576f, 1f);
            currentAngleText.text = hasSamples ? currentAngle.ToString("F1") + "°" : "--";
            maxAngleText.text = hasSamples ? maxAngle.ToString("F1") + "°" : "--";
            minAngleText.text = hasSamples ? minAngle.ToString("F1") + "°" : "--";
            elapsedText.text = "采集时长：" + FormatTime(elapsed);
            sampleCountText.text = "有效采样：" + sampleCount;
            measurementStateText.text = measuring ? "正在采集真实运动数据" : hasSamples ? "采集已暂停，可保存归档" : RehabTelemetryHub.IsConnected ? "等待开始采集" : "等待机械臂通讯接入";
            measureTitleText.text = GetJointName() + " · " + GetAxisName() + "测量";
            resultPatientText.text = Global.CurPatientInfo == null ? "当前患者：未选择" : "当前患者：" + Global.CurPatientInfo.Name + "（" + Global.CurPatientInfo.CardNum + "）";
            resultSelectionText.text = "评估配置：" + GetJointName() + " · " + (arom ? "AROM" : "PROM") + " · " + GetAxisName();
            resultRangeText.text = hasSamples ? "活动度：" + minAngle.ToString("F1") + "° ~ " + maxAngle.ToString("F1") + "°" : "活动度：--";
            archiveStateText.text = archived ? "归档状态：已写入报告数据库" : "归档状态：尚未归档";
            archiveStateText.color = archived ? new Color(0.086f, 0.639f, 0.29f, 1f) : new Color(0.557f, 0.557f, 0.576f, 1f);

            if (gaugePointer != null)
            {
                gaugePointer.gameObject.SetActive(false);
            }
        }

        private void CreateGaugeTicks()
        {
            if (gaugeTicks == null || gaugeTickTemplate == null) return;
            gaugeTickTemplate.SetActive(false);
            for (int i = 0; i <= 18; i++)
            {
                GameObject tick = UnityEngine.Object.Instantiate(gaugeTickTemplate, gaugeTicks);
                tick.name = "GaugeTick_" + (i * 10);
                tick.SetActive(true);
                RectTransform rect = tick.GetComponent<RectTransform>();
                float angle = i * 10f;
                float theta = angle * Mathf.Deg2Rad;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(Mathf.Cos(theta) * 120f, Mathf.Sin(theta) * 120f);
                Image image = tick.GetComponent<Image>();
                if (image != null) image.color = i % 3 == 0 ? new Color(0.557f, 0.557f, 0.576f, 1f) : new Color(0.796f, 0.816f, 0.859f, 1f);
            }
        }

        private string GetJointName()
        {
            if (joint == AssessmentJoint.Elbow) return "肘关节";
            if (joint == AssessmentJoint.Forearm) return "前臂旋转";
            return "肩关节";
        }

        private string GetAxisName()
        {
            if (axis == "abduction") return "内收 / 外展";
            if (axis == "horizontal") return "水平内收 / 外展";
            if (axis == "rotation") return "内旋 / 外旋";
            return "前屈 / 后伸";
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        private static void SetSelected(Button button, bool selected)
        {
            if (button == null) return;
            Image image = button.GetComponent<Image>();
            if (image != null) image.color = selected ? SelectedBackground : NormalBackground;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = selected ? SelectedText : NormalText;
        }

        private static void SetButtonText(Button button, string value)
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = value;
        }

        public override void OnPlaceTop()
        {
            base.OnPlaceTop();
            var mainWindow = Main.m_UI.GetOpenedUI<UIMain>();
            if (mainWindow != null)
            {
                UIEntity.transform.SetParent(mainWindow.Content);
                UIEntity.transform.localScale = Vector3.one;
                UIEntity.transform.localPosition = Vector3.zero;
            }
        }

        public override void OnClose()
        {
            measuring = false;
            base.OnClose();
        }

        public override void OnDestroy()
        {
            shoulderButton.onClick.RemoveAllListeners();
            elbowButton.onClick.RemoveAllListeners();
            forearmButton.onClick.RemoveAllListeners();
            aromButton.onClick.RemoveAllListeners();
            promButton.onClick.RemoveAllListeners();
            flexionAxisButton.onClick.RemoveAllListeners();
            abductionAxisButton.onClick.RemoveAllListeners();
            horizontalAxisButton.onClick.RemoveAllListeners();
            rotationAxisButton.onClick.RemoveAllListeners();
            startButton.onClick.RemoveAllListeners();
            saveButton.onClick.RemoveAllListeners();
            RehabTelemetryHub.ConnectionChanged -= OnConnectionChanged;
            RehabTelemetryHub.SampleReceived -= OnSampleReceived;
            RehabTelemetryHub.MetricsReceived -= OnMetricsReceived;
            base.OnDestroy();
        }
    }
}
