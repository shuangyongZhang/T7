using System;
using System.Collections.Generic;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    internal enum TrainingModeGroup
    {
        Active,
        Passive,
        Intelligent
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
        [InjectPath("ConsolePanel/ModeCard")] private GameObject modeCard;
        [InjectPath("ConsolePanel/GameListCard")] private GameObject gameListCard;
        [InjectPath("ConsolePanel/DirectionCard")] private GameObject directionCard;
        [InjectPath("ConsolePanel/ParameterCard")] private GameObject parameterCard;
        [InjectPath("ConsolePanel/TrainingGroupCard/ActiveGroupButton")] private Button activeGroupButton;
        [InjectPath("ConsolePanel/TrainingGroupCard/PassiveGroupButton")] private Button passiveGroupButton;
        [InjectPath("ConsolePanel/TrainingGroupCard/IntelligentGroupButton")] private Button intelligentGroupButton;
        [InjectPath("ConsolePanel/ModeCard/CpmButton")] private Button cpmButton;
        [InjectPath("ConsolePanel/ModeCard/AssistButton")] private Button assistButton;
        [InjectPath("ConsolePanel/ModeCard/ActiveButton")] private Button activeButton;
        [InjectPath("ConsolePanel/ModeCard/ResistButton")] private Button resistButton;
        [InjectPath("ConsolePanel/ModeCard/TeachingButton")] private Button teachingButton;
        [InjectPath("ConsolePanel/GameListCard/TrainingGameScroll")] private ScrollRect trainingGameScroll;
        [InjectPath("ConsolePanel/GameListCard/TrainingGameScroll/Viewport/Content")] private RectTransform trainingGameRowsContent;
        [InjectPath("ConsolePanel/GameListCard/TrainingGameScroll/Viewport/Content/TrainingGameRowTemplate")] private GameObject trainingGameRowTemplate;
        [InjectPath("ConsolePanel/GameListCard/TrainingGameScroll/Viewport/Content/TrainingGameEmptyText")] private TMP_Text trainingGameEmptyText;
        [InjectPath("ConsolePanel/DirectionCard/AbductionButton")] private Button abductionButton;
        [InjectPath("ConsolePanel/DirectionCard/FlexionButton")] private Button flexionButton;
        [InjectPath("ConsolePanel/DirectionCard/HorizontalButton")] private Button horizontalButton;
        [InjectPath("ConsolePanel/DirectionCard/PronationButton")] private Button pronationButton;
        [InjectPath("ConsolePanel/DirectionCard/RotationButton")] private Button rotationButton;
        [InjectPath("ConsolePanel/DirectionCard/ExtensionButton")] private Button extensionButton;
        [InjectPath("ConsolePanel/DirectionCard/DirectionLimitToggle")] private Toggle directionLimitToggle;
        [InjectPath("ConsolePanel/DirectionCard/DirectionLimitToggle/Background")] private Image directionLimitTrack;
        [InjectPath("ConsolePanel/DirectionCard/DirectionLimitToggle/Background/Knob")] private Image directionLimitKnob;
        [InjectPath("ConsolePanel/ParameterCard/ResistanceSlider")] private Slider resistanceSlider;
        [InjectPath("ConsolePanel/ParameterCard/DurationSlider")] private Slider durationSlider;
        [InjectPath("ConsolePanel/ParameterCard/DurationCurrentText")] private TMP_Text durationCurrentText;
        [InjectPath("ConsolePanel/ParameterCard/ResistanceValueText")] private TMP_Text resistanceValueText;
        [InjectPath("CanvasPanel/TrainingCard/SelectionSummaryCard/TrainingGameSummaryText")] private TMP_Text gameStatusText;
        [InjectPath("ConsolePanel/ActionCard/StartButton")] private Button startButton;
        [InjectPath("ConsolePanel/ActionCard/EmergencyStopButton")] private Button emergencyStopButton;
        [InjectPath("ConsolePanel/ActionCard/TimerText")] private TMP_Text timerText;
        [InjectPath("ConsolePanel/StatusBadge")] private Image statusBadge;
        [InjectPath("ConsolePanel/StatusBadge/StatusBadgeText")] private TMP_Text statusBadgeText;
        [InjectPath("ConsolePanel/SessionCard/TrainingStateText")] private TMP_Text trainingStateText;
        [InjectPath("ConsolePanel/SessionCard/ElapsedText")] private TMP_Text elapsedText;
        [InjectPath("ConsolePanel/SessionCard/SampleCountText")] private TMP_Text sampleCountText;
        [InjectPath("ConsolePanel/SessionCard/DeviceStateText")] private TMP_Text deviceStateText;
        [InjectPath("CanvasPanel/TrainingCard/ResultCard/ResultPatientText")] private TMP_Text patientStateText;
        [InjectPath("CanvasPanel/TrainingCard/ResultCard/ResultSelectionText")] private TMP_Text resultSelectionText;
        [InjectPath("CanvasPanel/TrainingCard/ResultCard/ResultRangeText")] private TMP_Text resultRangeText;
        [InjectPath("CanvasPanel/TrainingCard/ResultCard/ArchiveStateText")] private TMP_Text archiveStateText;
        [InjectPath("CanvasPanel/TrainingCard/SelectionSummaryCard/TrainingGroupSummaryText")] private TMP_Text trainingGroupSummaryText;
        [InjectPath("CanvasPanel/TrainingCard/SelectionSummaryCard/TrainingModeSummaryText")] private TMP_Text trainingModeSummaryText;
        [InjectPath("CanvasPanel/TrainingCard/SelectionSummaryCard/TrainingDirectionSummaryText")] private TMP_Text trainingDirectionSummaryText;
        [InjectPath("CanvasPanel/TrainingCard/SelectionSummaryCard/TrainingDurationSummaryText")] private TMP_Text trainingDurationSummaryText;
        [InjectPath("CanvasPanel/TrainingCard/DurationMetric/DurationValueText")] private TMP_Text durationValueText;
        [InjectPath("CanvasPanel/TrainingCard/ResistanceMetric/ResistanceMetricText")] private TMP_Text resistanceMetricText;
        [InjectPath("CanvasPanel/TrainingCard/GameMetric/GameMetricText")] private TMP_Text gameMetricText;

        private static readonly Color SelectedBackground = new Color(0.937f, 0.965f, 1f, 1f);
        private static readonly Color SelectedText = new Color(0.078f, 0.337f, 0.941f, 1f);
        private static readonly Color NormalText = new Color(0.271f, 0.318f, 0.369f, 1f);
        private static readonly Color DarkButton = new Color(0.094f, 0.118f, 0.145f, 1f);

        private TrainingMode mode = TrainingMode.Resist;
        private TrainingModeGroup modeGroup = TrainingModeGroup.Active;
        private string activeGameId;
        private TrainingSessionState sessionState = TrainingSessionState.Idle;
        private HashSet<string> selectedDirections = new HashSet<string> { "abduction" };
        private bool isDirectionRestricted;
        private int resistanceLevel = 3;
        private int durationMinutes = 20;
        private float elapsed;
        private float minAngle;
        private float maxAngle;
        private int sampleCount;
        private float refreshAccumulator;
        private RehabSessionRecorder recorder = new RehabSessionRecorder();
        private bool archived;

        public override void OnInit()
        {
            base.OnInit();
            RehabTelemetryHub.ConnectionChanged += OnConnectionChanged;
            RehabTelemetryHub.SampleReceived += OnSampleReceived;
            RehabTelemetryHub.MetricsReceived += OnMetricsReceived;

            activeGroupButton.onClick.AddListener(() => SelectModeGroup(TrainingModeGroup.Active));
            passiveGroupButton.onClick.AddListener(() => SelectModeGroup(TrainingModeGroup.Passive));
            intelligentGroupButton.onClick.AddListener(() => SelectModeGroup(TrainingModeGroup.Intelligent));
            cpmButton.onClick.AddListener(() => SelectMode(TrainingMode.Cpm));
            assistButton.onClick.AddListener(() => SelectMode(TrainingMode.Assist));
            activeButton.onClick.AddListener(() => SelectMode(TrainingMode.Active));
            resistButton.onClick.AddListener(() => SelectMode(TrainingMode.Resist));
            teachingButton.onClick.AddListener(() => SelectMode(TrainingMode.Teaching));
            abductionButton.onClick.AddListener(() => SelectDirection("abduction"));
            flexionButton.onClick.AddListener(() => SelectDirection("flexion"));
            horizontalButton.onClick.AddListener(() => SelectDirection("horizontal"));
            pronationButton.onClick.AddListener(() => SelectDirection("pronation"));
            rotationButton.onClick.AddListener(() => SelectDirection("rotation"));
            extensionButton.onClick.AddListener(() => SelectDirection("extension"));
            directionLimitToggle.onValueChanged.AddListener(OnDirectionLimitChanged);
            startButton.onClick.AddListener(ToggleSession);
            emergencyStopButton.onClick.AddListener(EmergencyStop);

            resistanceSlider.minValue = 1f;
            resistanceSlider.maxValue = 10f;
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
                RefreshDurationSelection();
                RefreshUI();
            });

            SelectMode(TrainingMode.Resist);
            RefreshDirectionSelection();
            RefreshDirectionLimitVisual();
            RefreshDurationSelection();
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
            SetSelected(activeGroupButton, modeGroup == TrainingModeGroup.Active);
            SetSelected(passiveGroupButton, modeGroup == TrainingModeGroup.Passive);
            SetSelected(intelligentGroupButton, modeGroup == TrainingModeGroup.Intelligent);
            SetSelected(cpmButton, mode == TrainingMode.Cpm);
            SetSelected(assistButton, mode == TrainingMode.Assist);
            SetSelected(activeButton, mode == TrainingMode.Active);
            SetSelected(resistButton, mode == TrainingMode.Resist);
            SetSelected(teachingButton, mode == TrainingMode.Teaching);
            RefreshModeVisibility();
            RefreshCategoryVisibility();
        }

        private void SelectModeGroup(TrainingModeGroup group)
        {
            modeGroup = group;
            if (!IsModeAllowed(mode))
            {
                mode = group == TrainingModeGroup.Passive
                    ? TrainingMode.Cpm
                    : group == TrainingModeGroup.Active ? TrainingMode.Active : TrainingMode.Intelligent;
            }
            SelectMode(mode);
        }

        private bool IsModeAllowed(TrainingMode value)
        {
            if (modeGroup == TrainingModeGroup.Intelligent) return value == TrainingMode.Intelligent;
            if (modeGroup == TrainingModeGroup.Active) return value == TrainingMode.Active || value == TrainingMode.Assist || value == TrainingMode.Resist;
            return value == TrainingMode.Cpm || value == TrainingMode.Teaching;
        }

        private void RefreshModeVisibility()
        {
            Button[] buttons = { activeButton, assistButton, resistButton, cpmButton, teachingButton };
            TrainingMode[] modes = { TrainingMode.Active, TrainingMode.Assist, TrainingMode.Resist, TrainingMode.Cpm, TrainingMode.Teaching };
            RectTransform firstRect = activeButton.GetComponent<RectTransform>();
            RectTransform cardRect = activeButton.transform.parent as RectTransform;
            float x = 16f + firstRect.rect.width * 0.5f - cardRect.rect.width * 0.5f;
            for (int i = 0; i < buttons.Length; i++)
            {
                bool allowed = IsModeAllowed(modes[i]);
                buttons[i].gameObject.SetActive(allowed);
                if (!allowed) continue;
                RectTransform rect = buttons[i].GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = new Vector2(x, cardRect.rect.height * 0.5f - 52f - rect.rect.height * 0.5f);
                x += 94f;
            }
        }

        private void RefreshCategoryVisibility()
        {
            bool intelligent = modeGroup == TrainingModeGroup.Intelligent;
            modeCard.SetActive(!intelligent);
            gameListCard.SetActive(!intelligent);
            directionCard.SetActive(intelligent);
            parameterCard.SetActive(intelligent);
            if (!intelligent) RefreshTrainingGames();
        }

        private void RefreshTrainingGames()
        {
            if (trainingGameRowsContent == null || trainingGameRowTemplate == null) return;
            for (int i = trainingGameRowsContent.childCount - 1; i >= 0; i--)
            {
                Transform child = trainingGameRowsContent.GetChild(i);
                if (child == trainingGameRowTemplate.transform || child == trainingGameEmptyText.transform) continue;
                child.gameObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }

            List<GameDefinition> games = new List<GameDefinition>();
            foreach (GameDefinition game in GameServices.Games)
            {
                if (game == null || game.Modes == null || Array.IndexOf(game.Modes, mode) < 0) continue;
                games.Add(game);
            }
            if (!games.Exists(game => game.Id == activeGameId))
                activeGameId = games.Count > 0 ? games[0].Id : null;

            trainingGameEmptyText.gameObject.SetActive(games.Count == 0);
            trainingGameEmptyText.text = GameServices.Catalog == null
                ? "游戏目录接口尚未接入"
                : "当前训练模式没有匹配游戏";
            RectTransform templateRect = trainingGameRowTemplate.GetComponent<RectTransform>();
            for (int i = 0; i < games.Count; i++)
            {
                GameDefinition game = games[i];
                GameObject row = UnityEngine.Object.Instantiate(trainingGameRowTemplate, trainingGameRowsContent);
                row.name = "TrainingGameRow_" + game.Id;
                row.SetActive(true);
                RectTransform rect = row.GetComponent<RectTransform>();
                if (rect != null && templateRect != null)
                    rect.anchoredPosition = new Vector2(templateRect.anchoredPosition.x, templateRect.anchoredPosition.y - i * 60f);
                SetChildText(row, "RowGameNameText", string.IsNullOrEmpty(game.DisplayName) ? game.Id : game.DisplayName);

                GameDefinition captured = game;
                Transform selectNode = row.transform.Find("SelectTrainingGameButton");
                if (selectNode != null)
                {
                    Button selectButton = selectNode.GetComponent<Button>();
                    selectButton.onClick.RemoveAllListeners();
                    selectButton.onClick.AddListener(() =>
                    {
                        activeGameId = captured.Id;
                        RefreshTrainingGameSelection();
                    });
                }
            }
            trainingGameRowsContent.sizeDelta = new Vector2(trainingGameRowsContent.sizeDelta.x, Mathf.Max(104f, games.Count * 60f + 8f));
            RefreshTrainingGameSelection();
        }

        private void RefreshTrainingGameSelection()
        {
            for (int i = 0; i < trainingGameRowsContent.childCount; i++)
            {
                Transform child = trainingGameRowsContent.GetChild(i);
                if (child == trainingGameRowTemplate.transform || child == trainingGameEmptyText.transform) continue;
                bool selected = child.name == "TrainingGameRow_" + activeGameId;
                Image image = child.GetComponent<Image>();
                if (image != null) image.color = selected ? SelectedBackground : Color.white;
                Transform selectNode = child.Find("SelectTrainingGameButton");
                if (selectNode != null)
                    SetButtonText(selectNode.GetComponent<Button>(), selected ? "已选择" : "选择游戏");
            }
            if (trainingGameScroll != null) trainingGameScroll.verticalNormalizedPosition = 1f;
        }

        private void SelectDirection(string value)
        {
            if (isDirectionRestricted)
            {
                selectedDirections.Clear();
                selectedDirections.Add(value);
            }
            else if (selectedDirections.Contains(value))
            {
                if (selectedDirections.Count > 1) selectedDirections.Remove(value);
            }
            else selectedDirections.Add(value);
            RefreshDirectionSelection();
        }

        private void OnDirectionLimitChanged(bool limited)
        {
            isDirectionRestricted = limited;
            if (limited && selectedDirections.Count > 1)
            {
                string first = null;
                foreach (string direction in TrainingCatalog.DirectionIds)
                {
                    if (selectedDirections.Contains(direction))
                    {
                        first = direction;
                        break;
                    }
                }
                selectedDirections.Clear();
                if (first != null) selectedDirections.Add(first);
            }
            RefreshDirectionSelection();
            RefreshDirectionLimitVisual();
        }

        private void RefreshDirectionLimitVisual()
        {
            if (directionLimitTrack != null)
                directionLimitTrack.color = directionLimitToggle.isOn ? SelectedText : new Color(0.82f, 0.835f, 0.86f, 1f);
            if (directionLimitKnob != null)
                directionLimitKnob.rectTransform.anchoredPosition = new Vector2(directionLimitToggle.isOn ? 12f : -12f, 0f);
        }

        private void RefreshDirectionSelection()
        {
            SetSelected(abductionButton, selectedDirections.Contains("abduction"));
            SetSelected(flexionButton, selectedDirections.Contains("flexion"));
            SetSelected(horizontalButton, selectedDirections.Contains("horizontal"));
            SetSelected(pronationButton, selectedDirections.Contains("pronation"));
            SetSelected(rotationButton, selectedDirections.Contains("rotation"));
            SetSelected(extensionButton, selectedDirections.Contains("extension"));
            RefreshUI();
        }

        private void RefreshDurationSelection()
        {
            if (durationSlider != null) durationSlider.SetValueWithoutNotify(durationMinutes);
            if (durationCurrentText != null) durationCurrentText.text = durationMinutes + " 分钟";
        }

        private void ToggleSession()
        {
            ToggleSessionCore();
        }

        private void ToggleSessionCore()
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
            if (GameServices.Runtime != null && GameServices.Runtime.IsAvailable)
            {
                GameSessionConfig config = new GameSessionConfig
                {
                    PatientId = Global.CurPatientInfo.ID,
                    GameId = activeGameId,
                    Mode = mode,
                    Modes = new[] { mode },
                    Directions = GetSelectedDirections(),
                    IsDirectionRestricted = isDirectionRestricted,
                    DurationMinutes = durationMinutes,
                    ResistanceLevel = resistanceLevel
                };
                string message;
                if (!GameServices.Runtime.TryStart(config, out message))
                {
                    UINotifyDialogHelper.Current.ShowPromptDialog("提示", message, "确定");
                    return;
                }
            }
            sessionState = TrainingSessionState.Running;
            SetButtonText(startButton, "结束并归档");
            startButton.interactable = false;
            RefreshUI();
        }

        private void CompleteSession()
        {
            sessionState = TrainingSessionState.Completed;
            if (GameServices.Runtime != null && GameServices.Runtime.IsAvailable) GameServices.Runtime.Stop();
            SetButtonText(startButton, "重新启动");
            startButton.interactable = RehabTelemetryHub.IsConnected;
            RefreshUI();
            ArchiveSession();
        }

        private void EmergencyStop()
        {
            bool wasRunning = sessionState == TrainingSessionState.Running;
            sessionState = TrainingSessionState.Stopped;
            if (GameServices.Runtime != null && GameServices.Runtime.IsAvailable) GameServices.Runtime.Stop();
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
            float angle = sample.CurrentAngleDegrees;
            minAngle = sampleCount == 1 ? angle : Mathf.Min(minAngle, angle);
            maxAngle = sampleCount == 1 ? angle : Mathf.Max(maxAngle, angle);
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
                Mode = TrainingCatalog.GetModeReportName(mode),
                Direction = string.Join(",", GetSelectedDirections()),
                ResistanceModel = "等速自适应",
                ResistanceLevel = resistanceLevel,
                DurationSeconds = elapsed,
                SampleCount = recorder.SampleCount,
                GameId = activeGameId,
                HasAffectedSide = true,
                AffectedSide = Global.CurPatientInfo.AffectedSide,
                HasDirectionRestriction = true,
                IsDirectionRestricted = isDirectionRestricted,
                Movement = recorder.CalculateMovement(),
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

        private string[] GetSelectedDirections()
        {
            List<string> directions = new List<string>();
            foreach (string direction in TrainingCatalog.DirectionIds)
            {
                if (selectedDirections.Contains(direction)) directions.Add(direction);
            }
            return directions.ToArray();
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
            SetButtonText(startButton, "启动训练");
            startButton.GetComponent<Image>().color = DarkButton;
            startButton.interactable = RehabTelemetryHub.IsConnected;
        }

        private void RefreshUI()
        {
            deviceStateText.text = RehabTelemetryHub.IsConnected ? "设备状态：真实数据已连接" : "设备状态：通讯未接入";
            deviceStateText.color = RehabTelemetryHub.IsConnected ? new Color(0.086f, 0.639f, 0.29f, 1f) : new Color(0.557f, 0.557f, 0.576f, 1f);
            patientStateText.text = Global.CurPatientInfo == null ? "当前患者：未选择" : "当前患者：" + Global.CurPatientInfo.Name + "（" + Global.CurPatientInfo.CardNum + "）";
            trainingStateText.text = sessionState == TrainingSessionState.Running
                ? "正在记录真实训练数据"
                : sessionState == TrainingSessionState.Paused ? "训练已暂停"
                : sessionState == TrainingSessionState.Stopped ? "训练已急停"
                : sessionState == TrainingSessionState.Completed ? "训练已完成"
                : RehabTelemetryHub.IsConnected ? "等待开始训练" : "等待机械臂通讯接入";
            elapsedText.text = "训练时长：" + FormatTime(elapsed);
            sampleCountText.text = "有效采样：" + sampleCount;
            timerText.text = FormatTime(elapsed) + " / " + durationMinutes + ":00";

            string runtimeStatus = GameServices.Runtime != null && GameServices.Runtime.IsAvailable ? "游戏接口已连接" : "游戏接口待接入";
            GameDefinition selectedGame = null;
            foreach (GameDefinition game in GameServices.Games)
            {
                if (game != null && game.Id == activeGameId)
                {
                    selectedGame = game;
                    break;
                }
            }
            string gameName = selectedGame == null ? null : string.IsNullOrEmpty(selectedGame.DisplayName) ? selectedGame.Id : selectedGame.DisplayName;
            gameMetricText.text = string.IsNullOrEmpty(gameName) ? "未选择" : gameName;
            gameStatusText.text = (string.IsNullOrEmpty(gameName) ? "未选择游戏" : gameName) + " · " + runtimeStatus;
            trainingGroupSummaryText.text = "训练分类：" + GetModeGroupName();
            trainingModeSummaryText.text = "训练模式：" + GetModeDisplayName(mode);
            trainingDirectionSummaryText.text = "运动方向：" + TrainingCatalog.GetDirectionNames(GetSelectedDirections());
            trainingDurationSummaryText.text = "训练时长：" + durationMinutes + " 分钟";
            durationValueText.text = durationMinutes + " 分钟";
            if (durationCurrentText != null) durationCurrentText.text = durationMinutes + " 分钟";
            resistanceValueText.text = GetResistanceText();
            resistanceMetricText.text = GetResistanceText();
            resultSelectionText.text = "训练配置：" + GetModeGroupName() + " · " + GetModeDisplayName(mode) + " · " +
                (modeGroup == TrainingModeGroup.Intelligent ? TrainingCatalog.GetDirectionNames(GetSelectedDirections()) : string.IsNullOrEmpty(gameName) ? "未选择游戏" : gameName);
            resultRangeText.text = sampleCount > 0 ? "活动度：" + minAngle.ToString("F1") + "° ~ " + maxAngle.ToString("F1") + "°" : "活动度：--";
            archiveStateText.text = archived ? "归档状态：已写入报告数据库" : "归档状态：尚未归档";
            archiveStateText.color = archived ? new Color(0.086f, 0.639f, 0.29f, 1f) : new Color(0.557f, 0.557f, 0.576f, 1f);

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
        }

        private string GetModeGroupName()
        {
            if (modeGroup == TrainingModeGroup.Passive) return "被动训练";
            if (modeGroup == TrainingModeGroup.Intelligent) return "智能训练";
            return "主动训练";
        }

        private static string GetModeDisplayName(TrainingMode value)
        {
            if (value == TrainingMode.Cpm) return "被动训练";
            if (value == TrainingMode.Assist) return "助力训练";
            if (value == TrainingMode.Active) return "主动训练";
            if (value == TrainingMode.Resist) return "抗阻训练";
            if (value == TrainingMode.Teaching) return "示教训练";
            if (value == TrainingMode.Intelligent) return "智能训练";
            return "未知训练";
        }

        private string GetResistanceText()
        {
            return "等级 " + resistanceLevel;
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

        private static void SetChildText(GameObject parent, string childName, string value)
        {
            Transform node = parent.transform.Find(childName);
            if (node == null) return;
            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text != null) text.text = value ?? "";
        }

        public override void OnPlaceTop()
        {
            base.OnPlaceTop();
            UIMain mainWindow = Main.m_UI.GetOpenedUI<UIMain>();
            if (mainWindow != null)
            {
                UIEntity.transform.SetParent(mainWindow.Content);
                UIEntity.transform.localScale = Vector3.one;
                UIEntity.transform.localPosition = Vector3.zero;
            }
        }

        public override void OnClose()
        {
            if (sessionState == TrainingSessionState.Running) CompleteSession();
            base.OnClose();
        }

        public override void OnDestroy()
        {
            activeGroupButton.onClick.RemoveAllListeners();
            passiveGroupButton.onClick.RemoveAllListeners();
            intelligentGroupButton.onClick.RemoveAllListeners();
            cpmButton.onClick.RemoveAllListeners();
            assistButton.onClick.RemoveAllListeners();
            activeButton.onClick.RemoveAllListeners();
            resistButton.onClick.RemoveAllListeners();
            teachingButton.onClick.RemoveAllListeners();
            abductionButton.onClick.RemoveAllListeners();
            flexionButton.onClick.RemoveAllListeners();
            horizontalButton.onClick.RemoveAllListeners();
            pronationButton.onClick.RemoveAllListeners();
            rotationButton.onClick.RemoveAllListeners();
            extensionButton.onClick.RemoveAllListeners();
            directionLimitToggle.onValueChanged.RemoveAllListeners();
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
