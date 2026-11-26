using System;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    internal enum TrainingMode
    {
        Cpm,
        Assist,
        Resist
    }

    internal enum TrainingSessionState
    {
        Idle,
        Running,
        Paused,
        Stopped,
        Completed
    }

    [UIResource("UITrainingPanel")]
    public class UITraining : UILogicResident
    {
        [InjectPath("ConsolePanel/ModeCard/CpmButton")] private Button cpmButton;
        [InjectPath("ConsolePanel/ModeCard/AssistButton")] private Button assistButton;
        [InjectPath("ConsolePanel/ModeCard/ResistButton")] private Button resistButton;
        [InjectPath("ConsolePanel/DirectionCard/AbductionButton")] private Button abductionButton;
        [InjectPath("ConsolePanel/DirectionCard/FlexionButton")] private Button flexionButton;
        [InjectPath("ConsolePanel/DirectionCard/HorizontalButton")] private Button horizontalButton;
        [InjectPath("ConsolePanel/DirectionCard/PronationButton")] private Button pronationButton;
        [InjectPath("ConsolePanel/DirectionCard/RotationButton")] private Button rotationButton;
        [InjectPath("ConsolePanel/DirectionCard/ExtensionButton")] private Button extensionButton;
        [InjectPath("ConsolePanel/ParameterCard/IsotonicButton")] private Button isotonicButton;
        [InjectPath("ConsolePanel/ParameterCard/IsokineticButton")] private Button isokineticButton;
        [InjectPath("ConsolePanel/ParameterCard/ElasticButton")] private Button elasticButton;
        [InjectPath("ConsolePanel/ParameterCard/ViscousButton")] private Button viscousButton;
        [InjectPath("ConsolePanel/ParameterCard/ResistanceSlider")] private Slider resistanceSlider;
        [InjectPath("ConsolePanel/ParameterCard/DurationSlider")] private Slider durationSlider;
        [InjectPath("ConsolePanel/ActionCard/StartButton")] private Button startButton;
        [InjectPath("ConsolePanel/ActionCard/EmergencyStopButton")] private Button emergencyStopButton;
        [InjectPath("ConsolePanel/StatusBadge")] private Image statusBadge;
        [InjectPath("ConsolePanel/StatusBadge/StatusBadgeText")] private TMP_Text statusBadgeText;
        [InjectPath("ConsolePanel/ParameterCard/ParameterTitleText")] private TMP_Text parameterTitleText;
        [InjectPath("ConsolePanel/ParameterCard/ModelLabelText")] private TMP_Text modelLabelText;
        [InjectPath("ConsolePanel/ParameterCard/ResistanceValueText")] private TMP_Text resistanceValueText;
        [InjectPath("ConsolePanel/ParameterCard/DurationValueText")] private TMP_Text durationValueText;
        [InjectPath("ConsolePanel/ActionCard/TimerText")] private TMP_Text timerText;
        [InjectPath("ConsolePanel/DeviceCard/DeviceStateText")] private TMP_Text deviceStateText;
        [InjectPath("ConsolePanel/DeviceCard/PatientStateText")] private TMP_Text patientStateText;
        [InjectPath("CanvasPanel/AngleCard/AngleSubtitleText")] private TMP_Text angleSubtitleText;
        [InjectPath("CanvasPanel/AngleCard/ArmArea/ArmUpper")] private RectTransform armUpper;
        [InjectPath("CanvasPanel/AngleCard/ArmArea/ArmLower")] private RectTransform armLower;
        [InjectPath("CanvasPanel/AngleCard/ArmArea/ShoulderJoint")] private RectTransform shoulderJoint;
        [InjectPath("CanvasPanel/AngleCard/ArmArea/ElbowJoint")] private RectTransform elbowJoint;
        [InjectPath("CanvasPanel/AngleCard/ArmArea/HandJoint")] private RectTransform handJoint;
        [InjectPath("CanvasPanel/AngleCard/ArmArea/ArmAngleText")] private TMP_Text armAngleText;
        [InjectPath("CanvasPanel/TelemetryCard/AngleMetric/LiveAngleText")] private TMP_Text liveAngleText;
        [InjectPath("CanvasPanel/TelemetryCard/SpeedMetric/LiveSpeedText")] private TMP_Text liveSpeedText;
        [InjectPath("CanvasPanel/TelemetryCard/RangeMetric/LiveRangeText")] private TMP_Text liveRangeText;
        [InjectPath("CanvasPanel/TelemetryCard/SampleMetric/SampleCountText")] private TMP_Text sampleCountText;
        [InjectPath("CanvasPanel/WaveCard/WaveArea")] private RectTransform waveArea;
        [InjectPath("CanvasPanel/WaveCard/WaveArea/WaveBarTemplate")] private GameObject waveBarTemplate;
        [InjectPath("CanvasPanel/WaveCard/WaveArea/WaveEmptyText")] private GameObject waveEmptyText;

        private static readonly Color SelectedBackground = new Color(0.937f, 0.965f, 1f, 1f);
        private static readonly Color SelectedText = new Color(0.078f, 0.337f, 0.941f, 1f);
        private static readonly Color NormalText = new Color(0.271f, 0.318f, 0.369f, 1f);
        private static readonly Color DarkButton = new Color(0.094f, 0.118f, 0.145f, 1f);

        private TrainingMode mode = TrainingMode.Resist;
        private TrainingSessionState sessionState = TrainingSessionState.Idle;
        private string direction = "abduction";
        private string resistanceModel = "等速自适应";
        private int resistanceLevel = 3;
        private int durationMinutes = 20;
        private float elapsed;
        private float currentAngle;
        private float lastSpeed;
        private float minAngle;
        private float maxAngle;
        private int sampleCount;
        private float refreshAccumulator;
        private RectTransform[] waveBars;
        private float[] waveValues;
        private RehabSessionRecorder recorder = new RehabSessionRecorder();
        private bool archived;

        public override void OnInit()
        {
            base.OnInit();
            RehabTelemetryHub.ConnectionChanged += OnConnectionChanged;
            RehabTelemetryHub.SampleReceived += OnSampleReceived;
            RehabTelemetryHub.MetricsReceived += OnMetricsReceived;

            cpmButton.onClick.AddListener(() => SelectMode(TrainingMode.Cpm));
            assistButton.onClick.AddListener(() => SelectMode(TrainingMode.Assist));
            resistButton.onClick.AddListener(() => SelectMode(TrainingMode.Resist));
            abductionButton.onClick.AddListener(() => SelectDirection("abduction"));
            flexionButton.onClick.AddListener(() => SelectDirection("flexion"));
            horizontalButton.onClick.AddListener(() => SelectDirection("horizontal"));
            pronationButton.onClick.AddListener(() => SelectDirection("pronation"));
            rotationButton.onClick.AddListener(() => SelectDirection("rotation"));
            extensionButton.onClick.AddListener(() => SelectDirection("extension"));
            isotonicButton.onClick.AddListener(() => SelectResistanceModel("等张恒力", isotonicButton));
            isokineticButton.onClick.AddListener(() => SelectResistanceModel("等速自适应", isokineticButton));
            elasticButton.onClick.AddListener(() => SelectResistanceModel("弹性渐变", elasticButton));
            viscousButton.onClick.AddListener(() => SelectResistanceModel("黏滞阻尼", viscousButton));
            startButton.onClick.AddListener(ToggleSession);
            emergencyStopButton.onClick.AddListener(EmergencyStop);
            startButton.interactable = RehabTelemetryHub.IsConnected;

            resistanceSlider.minValue = 1f;
            resistanceSlider.maxValue = 4f;
            resistanceSlider.wholeNumbers = true;
            resistanceSlider.value = resistanceLevel;
            resistanceSlider.onValueChanged.AddListener(value =>
            {
                resistanceLevel = Mathf.RoundToInt(value);
                resistanceValueText.text = GetResistanceText();
            });
            durationSlider.minValue = 5f;
            durationSlider.maxValue = 30f;
            durationSlider.wholeNumbers = true;
            durationSlider.value = durationMinutes;
            durationSlider.onValueChanged.AddListener(value =>
            {
                durationMinutes = Mathf.RoundToInt(value);
                durationValueText.text = durationMinutes + " 分钟";
            });

            CreateWaveBars();
            SelectMode(TrainingMode.Resist);
            SelectDirection("abduction");
            SelectResistanceModel("等速自适应", isokineticButton);
            RefreshUI();
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            ResetSession();
            RefreshUI();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (sessionState == TrainingSessionState.Running)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= durationMinutes * 60f)
                {
                    elapsed = durationMinutes * 60f;
                    CompleteSession();
                    UINotifyDialogHelper.Current.ShowNotifyAutoHide("训练时间已完成");
                }
            }

            refreshAccumulator += Time.deltaTime;
            if (refreshAccumulator >= 0.1f)
            {
                refreshAccumulator = 0f;
                RefreshUI();
            }
        }

        private void SelectMode(TrainingMode value)
        {
            mode = value;
            SetSelected(cpmButton, mode == TrainingMode.Cpm);
            SetSelected(assistButton, mode == TrainingMode.Assist);
            SetSelected(resistButton, mode == TrainingMode.Resist);
            parameterTitleText.text = "03  " + (mode == TrainingMode.Cpm ? "CPM 被动模式细分配置" : mode == TrainingMode.Assist ? "主动助力模式细分配置" : "智能抗阻模式细分配置");
        }

        private void SelectDirection(string value)
        {
            direction = value;
            SetSelected(abductionButton, direction == "abduction");
            SetSelected(flexionButton, direction == "flexion");
            SetSelected(horizontalButton, direction == "horizontal");
            SetSelected(pronationButton, direction == "pronation");
            SetSelected(rotationButton, direction == "rotation");
            SetSelected(extensionButton, direction == "extension");
            RefreshUI();
        }

        private void SelectResistanceModel(string value, Button selected)
        {
            resistanceModel = value;
            SetSelected(isotonicButton, selected == isotonicButton);
            SetSelected(isokineticButton, selected == isokineticButton);
            SetSelected(elasticButton, selected == elasticButton);
            SetSelected(viscousButton, selected == viscousButton);
            modelLabelText.text = "力学模型：" + resistanceModel;
        }

        private void ToggleSession()
        {
            if (Global.CurPatientInfo == null)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("请先选择患者");
                return;
            }

            if (!RehabTelemetryHub.IsConnected)
            {
                UINotifyDialogHelper.Current.ShowNotifyAutoHide("机械臂通讯协议未接入，暂时无法开始训练");
                return;
            }

            if (sessionState == TrainingSessionState.Running)
            {
                CompleteSession();
                return;
            }

            recorder.Reset();
            archived = false;
            elapsed = 0f;
            sampleCount = 0;
            minAngle = 0f;
            maxAngle = 0f;
            currentAngle = 0f;
            lastSpeed = 0f;
            for (int i = 0; i < waveValues.Length; i++) waveValues[i] = 0f;
            sessionState = TrainingSessionState.Running;
            SetButtonText(startButton, "结束并归档");
            startButton.interactable = false;
            RefreshUI();
        }

        private void CompleteSession()
        {
            sessionState = TrainingSessionState.Completed;
            SetButtonText(startButton, "重新启动");
            startButton.interactable = RehabTelemetryHub.IsConnected;
            RefreshUI();
            ArchiveSession();
        }

        private void EmergencyStop()
        {
            bool wasRunning = sessionState == TrainingSessionState.Running;
            sessionState = TrainingSessionState.Stopped;
            SetButtonText(startButton, "重新启动");
            startButton.interactable = RehabTelemetryHub.IsConnected;
            RefreshUI();
            if (wasRunning) ArchiveSession();
            UINotifyDialogHelper.Current.ShowPromptDialog("提示", "软件训练会话已急停。当前版本未向设备下发运动指令，硬件急停请使用设备实体按钮。", "确定");
        }

        private void OnConnectionChanged(bool connected)
        {
            startButton.interactable = connected && sessionState != TrainingSessionState.Running;
            RefreshUI();
        }

        private void OnSampleReceived(RehabSample sample)
        {
            if (sessionState != TrainingSessionState.Running || !recorder.AddSample(sample)) return;
            sampleCount = recorder.SampleCount;
            currentAngle = sample.CurrentAngleDegrees;
            lastSpeed = sample.SpeedMetersPerSecond;
            minAngle = sampleCount == 1 ? currentAngle : Mathf.Min(minAngle, currentAngle);
            maxAngle = sampleCount == 1 ? currentAngle : Mathf.Max(maxAngle, currentAngle);
            for (int i = 0; i < waveValues.Length - 1; i++) waveValues[i] = waveValues[i + 1];
            waveValues[waveValues.Length - 1] = currentAngle;
            RefreshUI();
        }

        private void OnMetricsReceived(RehabStrengthMetrics strength, RehabBaselineMetrics baseline)
        {
            if (sessionState != TrainingSessionState.Running) return;
            recorder.Strength = strength;
            recorder.Baseline = baseline;
        }

        private void ArchiveSession()
        {
            if (archived || recorder.SampleCount == 0 || Global.CurPatientInfo == null) return;
            DateTime now = DateTime.Now;
            TrainingReportPayload payload = new TrainingReportPayload
            {
                Mode = mode == TrainingMode.Cpm ? "CPM" : mode == TrainingMode.Assist ? "Assist" : "Resist",
                Direction = direction,
                ResistanceModel = resistanceModel,
                ResistanceLevel = resistanceLevel,
                DurationSeconds = elapsed,
                SampleCount = recorder.SampleCount,
                Rom = recorder.CalculateRom(),
                Strength = recorder.Strength,
                Baseline = recorder.Baseline
            };
            RehabReport report = new RehabReport
            {
                Title = "上肢训练报告",
                PatientID = Global.CurPatientInfo.ID,
                Kind = RehabReportKind.Training,
                Joint = AssessmentJoint.Shoulder,
                PayloadJson = JsonUtility.ToJson(payload),
                StartTime = now.AddSeconds(-elapsed),
                EndTime = now,
                CreateTime = now
            };
            if (!DBManagerModule.Instance.InsertRehabReport(report, recorder.Samples))
            {
                UINotifyDialogHelper.Current.ShowPromptDialog("错误", "训练报告写入数据库失败", "确定");
                return;
            }
            archived = true;
            UINotifyDialogHelper.Current.ShowNotifyAutoHide("训练数据已归档，可在报告列表查看");
        }

        private void ResetSession()
        {
            sessionState = TrainingSessionState.Idle;
            recorder.Reset();
            archived = false;
            elapsed = 0f;
            sampleCount = 0;
            minAngle = 0f;
            maxAngle = 0f;
            currentAngle = 0f;
            lastSpeed = 0f;
            for (int i = 0; i < waveValues.Length; i++) waveValues[i] = 0f;
            waveEmptyText.SetActive(true);
            SetButtonText(startButton, "启动训练");
            startButton.GetComponent<Image>().color = DarkButton;
            startButton.interactable = RehabTelemetryHub.IsConnected;
        }

        private void RefreshUI()
        {
            deviceStateText.text = RehabTelemetryHub.IsConnected ? "设备状态：真实数据已连接" : "设备状态：通讯未接入";
            deviceStateText.color = RehabTelemetryHub.IsConnected ? new Color(0.086f, 0.639f, 0.29f, 1f) : new Color(0.557f, 0.557f, 0.576f, 1f);
            patientStateText.text = Global.CurPatientInfo == null ? "当前患者：未选择" : "当前患者：" + Global.CurPatientInfo.Name;
            liveAngleText.text = sampleCount > 0 ? currentAngle.ToString("F1") + "°" : "--";
            armAngleText.text = sampleCount > 0 ? currentAngle.ToString("F1") + "°" : "--";
            liveSpeedText.text = sampleCount > 0 ? lastSpeed.ToString("F1") + " m/s" : "--";
            liveRangeText.text = sampleCount > 0 ? minAngle.ToString("F1") + " ~ " + maxAngle.ToString("F1") : "-- ~ --";
            sampleCountText.text = sampleCount.ToString();
            timerText.text = FormatTime(elapsed) + " / " + durationMinutes + ":00";
            angleSubtitleText.text = "当前方向：" + GetDirectionName();

            string status;
            Color badgeColor;
            Color textColor;
            if (sessionState == TrainingSessionState.Running)
            {
                status = "正在运行";
                badgeColor = new Color(0.910f, 1f, 0.918f, 1f);
                textColor = new Color(0.086f, 0.639f, 0.29f, 1f);
            }
            else if (sessionState == TrainingSessionState.Paused)
            {
                status = "已暂停";
                badgeColor = new Color(0.941f, 0.941f, 0.941f, 1f);
                textColor = NormalText;
            }
            else if (sessionState == TrainingSessionState.Stopped)
            {
                status = "已急停";
                badgeColor = new Color(1f, 0.949f, 0.949f, 1f);
                textColor = new Color(0.863f, 0.149f, 0.149f, 1f);
            }
            else if (sessionState == TrainingSessionState.Completed)
            {
                status = "训练完成";
                badgeColor = new Color(0.910f, 1f, 0.918f, 1f);
                textColor = new Color(0.086f, 0.639f, 0.29f, 1f);
            }
            else
            {
                status = RehabTelemetryHub.IsConnected ? "待机" : "通讯未接入";
                badgeColor = new Color(0.941f, 0.941f, 0.941f, 1f);
                textColor = NormalText;
            }
            statusBadgeText.text = status;
            statusBadgeText.color = textColor;
            statusBadge.color = badgeColor;

            UpdateArm();
            UpdateWaveBars();
        }

        private void UpdateArm()
        {
            if (armUpper == null || armLower == null) return;
            float upperRotation = -Mathf.Clamp(currentAngle, -80f, 160f);
            float lowerRotation = upperRotation - 35f;
            Vector2 shoulder = shoulderJoint.anchoredPosition;
            armUpper.pivot = new Vector2(0f, 0.5f);
            armUpper.anchoredPosition = shoulder;
            armUpper.localEulerAngles = new Vector3(0f, 0f, upperRotation);
            Vector2 elbow = shoulder + new Vector2(Mathf.Cos(upperRotation * Mathf.Deg2Rad), Mathf.Sin(upperRotation * Mathf.Deg2Rad)) * armUpper.rect.width;
            armLower.pivot = new Vector2(0f, 0.5f);
            armLower.anchoredPosition = elbow;
            armLower.localEulerAngles = new Vector3(0f, 0f, lowerRotation);
            elbowJoint.anchoredPosition = elbow;
            Vector2 hand = elbow + new Vector2(Mathf.Cos(lowerRotation * Mathf.Deg2Rad), Mathf.Sin(lowerRotation * Mathf.Deg2Rad)) * armLower.rect.width;
            handJoint.anchoredPosition = hand;
        }

        private void CreateWaveBars()
        {
            if (waveArea == null || waveBarTemplate == null) return;
            waveBarTemplate.SetActive(false);
            waveBars = new RectTransform[48];
            waveValues = new float[waveBars.Length];
            for (int i = 0; i < waveBars.Length; i++)
            {
                GameObject bar = UnityEngine.Object.Instantiate(waveBarTemplate, waveArea);
                bar.name = "WaveBar_" + i;
                bar.SetActive(true);
                RectTransform rect = bar.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-waveArea.rect.width / 2f + (i + 0.5f) * waveArea.rect.width / waveBars.Length, 0f);
                rect.sizeDelta = new Vector2(8f, 4f);
                waveBars[i] = rect;
            }
        }

        private void UpdateWaveBars()
        {
            if (waveBars == null) return;
            for (int i = 0; i < waveBars.Length; i++)
            {
                waveBars[i].gameObject.SetActive(sessionState == TrainingSessionState.Running || sampleCount > 0);
                float normalized = Mathf.InverseLerp(-90f, 180f, waveValues[i]);
                float height = 4f + normalized * 150f;
                waveBars[i].sizeDelta = new Vector2(8f, height);
                Image image = waveBars[i].GetComponent<Image>();
                if (image != null) image.color = Color.Lerp(new Color(0.227f, 0.451f, 0.933f, 1f), new Color(0.478f, 0.675f, 0.980f, 1f), normalized);
            }
        }

        private string GetResistanceText()
        {
            if (resistanceLevel == 1) return "等级 1 (8 N)";
            if (resistanceLevel == 2) return "等级 2 (14 N)";
            if (resistanceLevel == 4) return "等级 4 (28 N)";
            return "等级 3 (20 N)";
        }

        private string GetDirectionName()
        {
            if (direction == "flexion") return "矢状面前屈";
            if (direction == "horizontal") return "水平面内收";
            if (direction == "pronation") return "冠状轴旋前";
            if (direction == "rotation") return "垂直轴旋转";
            if (direction == "extension") return "矢状轴屈伸";
            return "冠状面外展";
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
            if (image != null) image.color = selected ? SelectedBackground : Color.white;
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
            if (sessionState == TrainingSessionState.Running) sessionState = TrainingSessionState.Paused;
            base.OnClose();
        }

        public override void OnDestroy()
        {
            cpmButton.onClick.RemoveAllListeners();
            assistButton.onClick.RemoveAllListeners();
            resistButton.onClick.RemoveAllListeners();
            abductionButton.onClick.RemoveAllListeners();
            flexionButton.onClick.RemoveAllListeners();
            horizontalButton.onClick.RemoveAllListeners();
            pronationButton.onClick.RemoveAllListeners();
            rotationButton.onClick.RemoveAllListeners();
            extensionButton.onClick.RemoveAllListeners();
            isotonicButton.onClick.RemoveAllListeners();
            isokineticButton.onClick.RemoveAllListeners();
            elasticButton.onClick.RemoveAllListeners();
            viscousButton.onClick.RemoveAllListeners();
            resistanceSlider.onValueChanged.RemoveAllListeners();
            durationSlider.onValueChanged.RemoveAllListeners();
            startButton.onClick.RemoveAllListeners();
            emergencyStopButton.onClick.RemoveAllListeners();
            RehabTelemetryHub.ConnectionChanged -= OnConnectionChanged;
            RehabTelemetryHub.SampleReceived -= OnSampleReceived;
            RehabTelemetryHub.MetricsReceived -= OnMetricsReceived;
            base.OnDestroy();
        }
    }
}
