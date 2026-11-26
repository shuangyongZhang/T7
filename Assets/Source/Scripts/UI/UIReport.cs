using System;
using System.Collections.Generic;
using HT.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    [UIResource("UIReportPanel")]
    public class UIReport : UILogicResident
    {
        [InjectPath("ConsolePanel/CountBadge/ReportCountText")] private TMP_Text reportCountText;
        [InjectPath("ConsolePanel/PatientCard/PatientNameText")] private TMP_Text patientNameText;
        [InjectPath("ConsolePanel/PatientCard/PatientMRNText")] private TMP_Text patientMRNText;
        [InjectPath("ConsolePanel/PatientCard/PatientMetaText")] private TMP_Text patientMetaText;
        [InjectPath("ConsolePanel/FilterTabs/AllFilterButton")] private Button allFilterButton;
        [InjectPath("ConsolePanel/FilterTabs/TrainingFilterButton")] private Button trainingFilterButton;
        [InjectPath("ConsolePanel/FilterTabs/AssessmentFilterButton")] private Button assessmentFilterButton;
        [InjectPath("ConsolePanel/RefreshButton")] private Button refreshButton;
        [InjectPath("ConsolePanel/ReportRowsContent")] private RectTransform rowsContent;
        [InjectPath("ConsolePanel/ReportRowsContent/ReportRowTemplate")] private GameObject rowTemplate;
        [InjectPath("ConsolePanel/EmptyListState")] private GameObject emptyListState;
        [InjectPath("ConsolePanel/ListCountText")] private TMP_Text listCountText;
        [InjectPath("ConsolePanel/ReportPager")] private RectTransform reportPager;
        [InjectPath("ConsolePanel/ReportPager/PrevPageButton")] private Button prevPageButton;
        [InjectPath("ConsolePanel/ReportPager/NextPageButton")] private Button nextPageButton;
        [InjectPath("ConsolePanel/ReportPager/PageNumberTemplate")] private GameObject pageNumberTemplate;
        [InjectPath("ConsolePanel/ReportPager/PageEllipsisTemplate")] private GameObject pageEllipsisTemplate;
        [InjectPath("CanvasPanel/DetailCard/DetailTitleText")] private TMP_Text detailTitleText;
        [InjectPath("CanvasPanel/DetailCard/DetailTypeText")] private TMP_Text detailTypeText;
        [InjectPath("CanvasPanel/DetailCard/DetailDateText")] private TMP_Text detailDateText;
        [InjectPath("CanvasPanel/DetailCard/AssessmentScroll")] private ScrollRect assessmentScroll;
        [InjectPath("CanvasPanel/DetailCard/AssessmentScroll/Viewport/Content")] private RectTransform assessmentContent;
        [InjectPath("CanvasPanel/DetailCard/TrainingPlaceholder")] private GameObject trainingPlaceholder;
        [InjectPath("CanvasPanel/DetailCard/EmptyDetail")] private GameObject emptyDetail;

        private const int PageSize = 4;
        private const int MaxRomCards = 4;
        private const float RomCardStepY = 300f;
        private const float RomFlowStartY = 872f;
        private const float RowStep = 152f;
        private const float AssessmentScale = 1f;

        private static readonly Color SelectedBackground = new Color(0.937f, 0.965f, 1f, 1f);
        private static readonly Color SelectedText = new Color(0.078f, 0.337f, 0.941f, 1f);
        private static readonly Color NormalTabBackground = new Color(0.941f, 0.941f, 0.941f, 1f);
        private static readonly Color NormalText = new Color(0.271f, 0.318f, 0.369f, 1f);

        private List<RehabReport> reports;
        private Dictionary<int, List<Vector3>> romPointCache = new Dictionary<int, List<Vector3>>();
        private RehabReportKind? filter;
        private RehabReport selectedReport;
        private GameObject selectedRow;
        private int curPage;
        private GameObject assessmentReportInstance;
        private TMP_Text assessmentPatientNameText;
        private TMP_Text assessmentAffectedSideText;
        private TMP_Text assessmentDateText;
        private TMP_Text assessmentFooterText;
        private TMP_Text assessmentReportTitleText;
        private TMP_Text[] romMetricTexts;
        private TMP_Text[] romVolumeTexts;
        private TMP_Text[] romDateTexts;
        private RomRangeChart[] romCharts;
        private TMP_Text[][] romAxisLabels;
        private RectTransform[] romCards;
        private Vector2[] romCardBasePositions;
        private RectTransform[] reportFlowNodes;
        private Vector2[] reportFlowBasePositions;
        private RectTransform reportContentRect;
        private float reportContentBaseHeight;
        private TMP_Text romSectionHintText;
        private TMP_Text[] strengthLegendDateTexts;
        private TMP_Text[] strengthRowDateTexts;
        private TMP_Text[][] strengthValueTexts;
        private TMP_Text radarSummaryText;
        private ReportChartGraphic strengthChart;
        private TMP_Text[] baselineDateTexts;
        private ReportChartGraphic baselineChart;

        public override void OnInit()
        {
            base.OnInit();
            allFilterButton.onClick.AddListener(() => SelectFilter(null));
            trainingFilterButton.onClick.AddListener(() => SelectFilter(RehabReportKind.Training));
            assessmentFilterButton.onClick.AddListener(() => SelectFilter(RehabReportKind.Assessment));
            refreshButton.onClick.AddListener(RefreshReports);
            prevPageButton.onClick.AddListener(() => TurnPage(-1));
            nextPageButton.onClick.AddListener(() => TurnPage(1));
            CreateAssessmentReportInstance();
        }

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            curPage = 0;
            RefreshReports();
        }

        private void CreateAssessmentReportInstance()
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/UIAssessmentReportTemplate");
            if (prefab == null)
            {
                Debug.LogError("[UIReport] 找不到评估报告内容模板 Prefab。");
                return;
            }

            assessmentScroll.horizontal = false;
            assessmentScroll.vertical = true;

            assessmentReportInstance = UnityEngine.Object.Instantiate(prefab, assessmentContent);
            assessmentReportInstance.name = "AssessmentReportInstance";
            RectTransform rect = assessmentReportInstance.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = new Vector3(AssessmentScale, AssessmentScale, 1f);
            assessmentContent.sizeDelta = new Vector2(assessmentContent.rect.width, rect.rect.height * AssessmentScale);
            assessmentReportInstance.SetActive(false);

            Transform content = assessmentReportInstance.transform.Find("ReportContent");
            assessmentPatientNameText = FindText(content, "PatientNameText");
            assessmentAffectedSideText = FindText(content, "AffectedSideText");
            assessmentDateText = FindText(content, "ReportDateText");
            assessmentFooterText = FindText(content, "ReportFooterLeftText");
            NormalizeReportContentLayout(content);
            CacheAssessmentReportNodes(content);
        }

        private static void NormalizeReportContentLayout(Transform content)
        {
            RectTransform parent = content as RectTransform;
            if (parent == null) return;
            float width = parent.rect.width;
            float height = parent.rect.height;
            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform child = content.GetChild(i) as RectTransform;
                if (child == null) continue;
                float left = width * child.anchorMin.x + child.anchoredPosition.x - child.sizeDelta.x * child.pivot.x;
                float top = height * (1f - child.anchorMax.y) - child.anchoredPosition.y - child.sizeDelta.y * (1f - child.pivot.y);
                child.anchorMin = child.anchorMax = new Vector2(0f, 1f);
                child.pivot = new Vector2(0f, 1f);
                child.anchoredPosition = new Vector2(left, -top);
            }
            parent.anchorMin = parent.anchorMax = new Vector2(0f, 1f);
            parent.pivot = new Vector2(0f, 1f);
            parent.anchoredPosition = Vector2.zero;
        }

        private void CacheAssessmentReportNodes(Transform content)
        {
            assessmentReportTitleText = FindText(content, "ReportTitleText");
            romMetricTexts = new[]
            {
                FindText(content, "RomCardA/RomCardAMetricText"), FindText(content, "RomCardB/RomCardBMetricText"),
                FindText(content, "RomCardC/RomCardCMetricText"), FindText(content, "RomCardD/RomCardDMetricText")
            };
            romVolumeTexts = new[]
            {
                FindText(content, "RomCardA/RomCardAVolumeText"), FindText(content, "RomCardB/RomCardBVolumeText"),
                FindText(content, "RomCardC/RomCardCVolumeText"), FindText(content, "RomCardD/RomCardDVolumeText")
            };
            romDateTexts = new[]
            {
                FindText(content, "RomCardA/RomCardADateText"), FindText(content, "RomCardB/RomCardBDateText"),
                FindText(content, "RomCardC/RomCardCDateText"), FindText(content, "RomCardD/RomCardDDateText")
            };
            romCharts = new[]
            {
                FindRomChart(content, "RomCardA/RomRangeChart"), FindRomChart(content, "RomCardB/RomRangeChart"),
                FindRomChart(content, "RomCardC/RomRangeChart"), FindRomChart(content, "RomCardD/RomRangeChart")
            };
            romAxisLabels = new[]
            {
                new[] { FindText(content, "RomCardA/RomRangeChart/RomVerticalAxisLabel"), FindText(content, "RomCardA/RomRangeChart/RomCoronalAxisLabel"), FindText(content, "RomCardA/RomRangeChart/RomSagittalAxisLabel") },
                new[] { FindText(content, "RomCardB/RomRangeChart/RomVerticalAxisLabel"), FindText(content, "RomCardB/RomRangeChart/RomCoronalAxisLabel"), FindText(content, "RomCardB/RomRangeChart/RomSagittalAxisLabel") },
                new[] { FindText(content, "RomCardC/RomRangeChart/RomVerticalAxisLabel"), FindText(content, "RomCardC/RomRangeChart/RomCoronalAxisLabel"), FindText(content, "RomCardC/RomRangeChart/RomSagittalAxisLabel") },
                new[] { FindText(content, "RomCardD/RomRangeChart/RomVerticalAxisLabel"), FindText(content, "RomCardD/RomRangeChart/RomCoronalAxisLabel"), FindText(content, "RomCardD/RomRangeChart/RomSagittalAxisLabel") }
            };
            romSectionHintText = FindText(content, "RomSectionHintText");
            strengthLegendDateTexts = new[]
            {
                FindText(content, "StrengthRadarCard/LegendBlueText"), FindText(content, "StrengthRadarCard/LegendOrangeText"),
                FindText(content, "StrengthRadarCard/LegendGreenText"), FindText(content, "StrengthRadarCard/LegendRedText")
            };
            strengthRowDateTexts = new[]
            {
                FindText(content, "StrengthTable/StrengthRow1Date"), FindText(content, "StrengthTable/StrengthRow2Date"),
                FindText(content, "StrengthTable/StrengthRow3Date")
            };
            strengthValueTexts = new[]
            {
                new[]
                {
                    FindText(content, "StrengthTable/StrengthRow1Flexion"), FindText(content, "StrengthTable/StrengthRow1Extension"),
                    FindText(content, "StrengthTable/StrengthRow1Up"), FindText(content, "StrengthTable/StrengthRow1Down"),
                    FindText(content, "StrengthTable/StrengthRow1Right"), FindText(content, "StrengthTable/StrengthRow1Left")
                },
                new[]
                {
                    FindText(content, "StrengthTable/StrengthRow2Flexion"), FindText(content, "StrengthTable/StrengthRow2Extension"),
                    FindText(content, "StrengthTable/StrengthRow2Up"), FindText(content, "StrengthTable/StrengthRow2Down"),
                    FindText(content, "StrengthTable/StrengthRow2Right"), FindText(content, "StrengthTable/StrengthRow2Left")
                },
                new[]
                {
                    FindText(content, "StrengthTable/StrengthRow3Flexion"), FindText(content, "StrengthTable/StrengthRow3Extension"),
                    FindText(content, "StrengthTable/StrengthRow3Up"), FindText(content, "StrengthTable/StrengthRow3Down"),
                    FindText(content, "StrengthTable/StrengthRow3Right"), FindText(content, "StrengthTable/StrengthRow3Left")
                }
            };
            radarSummaryText = FindText(content, "StrengthRadarCard/RadarSummaryText");
            strengthChart = FindChart(content, "StrengthRadarCard/StrengthRadarChart");
            baselineDateTexts = new[]
            {
                FindText(content, "BaselineCard/BaselineChart/BaselineDate1Text"), FindText(content, "BaselineCard/BaselineChart/BaselineDate2Text"),
                FindText(content, "BaselineCard/BaselineChart/BaselineDate3Text"), FindText(content, "BaselineCard/BaselineChart/BaselineDate4Text")
            };
            baselineChart = FindChart(content, "BaselineCard/BaselineChart");
            CacheRomLayout(content);
        }

        private void CacheRomLayout(Transform content)
        {
            romCards = new[]
            {
                content.Find("RomCardA") as RectTransform, content.Find("RomCardB") as RectTransform,
                content.Find("RomCardC") as RectTransform, content.Find("RomCardD") as RectTransform
            };
            romCardBasePositions = new Vector2[MaxRomCards];
            for (int i = 0; i < romCards.Length; i++)
            {
                if (romCards[i] != null) romCardBasePositions[i] = romCards[i].anchoredPosition;
            }

            reportContentRect = content as RectTransform;
            List<RectTransform> flowNodes = new List<RectTransform>();
            for (int i = 0; i < content.childCount; i++)
            {
                Transform child = content.GetChild(i);
                RectTransform rect = child as RectTransform;
                if (rect == null) continue;
                float top = reportContentRect.rect.height * (1f - rect.anchorMax.y) - rect.anchoredPosition.y - rect.sizeDelta.y * (1f - rect.pivot.y);
                if (top >= RomFlowStartY) flowNodes.Add(rect);
            }
            reportFlowNodes = flowNodes.ToArray();
            reportFlowBasePositions = new Vector2[reportFlowNodes.Length];
            for (int i = 0; i < reportFlowNodes.Length; i++) reportFlowBasePositions[i] = reportFlowNodes[i].anchoredPosition;

            reportContentBaseHeight = reportContentRect != null ? reportContentRect.rect.height : 2380f;
        }

        private void SelectFilter(RehabReportKind? value)
        {
            filter = value;
            curPage = 0;
            if (selectedReport != null && filter != null && selectedReport.Kind != filter.Value) selectedReport = null;
            SetSelected(allFilterButton, filter == null);
            SetSelected(trainingFilterButton, filter == RehabReportKind.Training);
            SetSelected(assessmentFilterButton, filter == RehabReportKind.Assessment);
            RenderRows(true);
        }

        private void RefreshReports()
        {
            reports = Global.CurPatientInfo == null
                ? new List<RehabReport>()
                : DBManagerModule.Instance.QueryRehabReports(Global.CurPatientInfo.ID) ?? new List<RehabReport>();
            romPointCache.Clear();
            reports.Sort((a, b) =>
            {
                int timeCompare = b.CreateTime.CompareTo(a.CreateTime);
                return timeCompare != 0 ? timeCompare : b.ReportID.CompareTo(a.ReportID);
            });

            if (Global.CurPatientInfo == null)
            {
                patientNameText.text = "未选择患者";
                patientMRNText.text = "病历号：--";
                patientMetaText.text = "请先在患者页面选择患者";
            }
            else
            {
                PatientInfo patient = Global.CurPatientInfo;
                patientNameText.text = patient.Name + " 档案";
                patientMRNText.text = "病历号：" + patient.CardNum;
                patientMetaText.text = (patient.Sex == Sex.Female ? "女" : "男") + " · " + patient.Age + " 岁 · " + GetSideName(patient.AffectedSide);
            }

            int trainingCount = 0;
            int assessmentCount = 0;
            foreach (RehabReport report in reports)
            {
                if (report.Kind == RehabReportKind.Training) trainingCount++;
                else assessmentCount++;
            }
            reportCountText.text = reports.Count + " 份报告";
            SetButtonText(allFilterButton, "全部 (" + reports.Count + ")");
            SetButtonText(trainingFilterButton, "训练 (" + trainingCount + ")");
            SetButtonText(assessmentFilterButton, "评估 (" + assessmentCount + ")");

            if (selectedReport != null && reports.FindIndex(r => r.ReportID == selectedReport.ReportID) < 0) selectedReport = null;
            RenderRows(true);
        }

        private void RenderRows(bool followSelection)
        {
            List<RehabReport> visible = new List<RehabReport>();
            if (reports != null)
            {
                foreach (RehabReport report in reports)
                {
                    if (filter == null || report.Kind == filter.Value) visible.Add(report);
                }
            }
            visible.Sort((a, b) =>
            {
                int timeCompare = b.CreateTime.CompareTo(a.CreateTime);
                return timeCompare != 0 ? timeCompare : b.ReportID.CompareTo(a.ReportID);
            });

            int pageCount = Mathf.Max(1, Mathf.CeilToInt(visible.Count / (float)PageSize));
            int keepIndex = selectedReport != null ? visible.FindIndex(r => r.ReportID == selectedReport.ReportID) : -1;
            if (followSelection && keepIndex >= 0) curPage = keepIndex / PageSize;
            curPage = Mathf.Clamp(curPage, 0, pageCount - 1);

            ClearRows();
            int start = curPage * PageSize;
            int sliceCount = Mathf.Min(PageSize, visible.Count - start);
            RectTransform templateRect = rowTemplate.GetComponent<RectTransform>();
            Vector2 basePosition = templateRect != null ? templateRect.anchoredPosition : Vector2.zero;
            for (int i = 0; i < sliceCount; i++)
            {
                RehabReport report = visible[start + i];
                GameObject row = UnityEngine.Object.Instantiate(rowTemplate, rowsContent);
                row.name = "ReportRow_" + i;
                row.SetActive(true);
                RectTransform rect = row.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(basePosition.x, basePosition.y - i * RowStep);

                SetChildText(row, "RowTitleText", report.Title);
                SetChildText(row, "RowTimeText", report.CreateTime == default(DateTime) ? "--" : report.CreateTime.ToString("yyyy-MM-dd HH:mm"));
                SetChildText(row, "RowMetricText", GetMetricText(report));
                SetChildText(row, "RowTypeText", report.Kind == RehabReportKind.Training ? "训练报告" : "ROM 评估");
                SetRowTypeColor(row, report.Kind);

                RehabReport captured = report;
                Button rowButton = row.GetComponent<Button>();
                if (rowButton == null) rowButton = row.AddComponent<Button>();
                rowButton.transition = Selectable.Transition.None;
                rowButton.targetGraphic = row.GetComponent<Image>();
                rowButton.onClick.RemoveAllListeners();
                rowButton.onClick.AddListener(() => SelectReport(captured, row));
            }

            emptyListState.SetActive(sliceCount == 0);
            listCountText.text = "共 " + visible.Count + " 条 · 第 " + (curPage + 1) + "/" + pageCount + " 页";
            if (sliceCount > 0)
            {
                int keepInSlice = keepIndex - start;
                if (keepIndex >= 0 && keepInSlice >= 0 && keepInSlice < sliceCount)
                {
                    SelectReport(visible[keepIndex], FindRowByIndex(keepInSlice));
                }
                else
                {
                    SelectReport(visible[start], FindRowByIndex(0));
                }
            }
            else
            {
                SelectReport(null, null);
            }

            UIPagerBar.Refresh(reportPager, prevPageButton, nextPageButton, pageNumberTemplate, pageEllipsisTemplate,
                pageCount, curPage, OnPageSelected);
        }

        private void SelectReport(RehabReport report, GameObject row)
        {
            selectedReport = report;
            selectedRow = row;
            HighlightSelectedRow();

            if (report == null)
            {
                detailTitleText.text = "请选择报告";
                detailTypeText.text = "报告内容将在这里显示";
                detailDateText.text = "--";
                assessmentScroll.gameObject.SetActive(false);
                trainingPlaceholder.SetActive(false);
                emptyDetail.SetActive(true);
                return;
            }

            detailTitleText.text = report.Title;
            detailTypeText.text = report.Kind == RehabReportKind.Training ? "训练报告" : "关节活动范围评估报告";
            detailDateText.text = report.CreateTime == default(DateTime) ? "--" : report.CreateTime.ToString("yyyy-MM-dd HH:mm");
            assessmentScroll.gameObject.SetActive(true);
            trainingPlaceholder.SetActive(false);
            emptyDetail.SetActive(false);
            BindRehabReport(report);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(assessmentScroll.viewport);
            assessmentScroll.content.anchoredPosition = new Vector2(assessmentScroll.content.anchoredPosition.x, 0f);
            assessmentScroll.verticalNormalizedPosition = 1f;
            assessmentScroll.velocity = Vector2.zero;
        }

        private void BindRehabReport(RehabReport report)
        {
            if (assessmentReportInstance == null) return;
            assessmentReportInstance.SetActive(true);
            PatientInfo patient = Global.CurPatientInfo;
            if (assessmentPatientNameText != null)
                assessmentPatientNameText.text = patient != null && !string.IsNullOrEmpty(patient.Name) ? patient.Name : "--";
            if (assessmentAffectedSideText != null)
                assessmentAffectedSideText.text = patient != null ? GetSideName(patient.AffectedSide) : "--";
            if (assessmentDateText != null)
                assessmentDateText.text = (report.CreateTime == default(DateTime) ? DateTime.Now : report.CreateTime).ToString("yyyy-MM-dd HH:mm");
            if (assessmentFooterText != null)
                assessmentFooterText.text = "报告编号：REP-" + report.ReportID.ToString("000000");
            if (assessmentReportTitleText != null)
                assessmentReportTitleText.text = report.Kind == RehabReportKind.Training ? "康复训练报告" : "康复评估报告";

            List<RehabReport> romHistory = GetRecentAssessmentHistory();
            BindRomCards(romHistory);
            List<RehabReport> history = GetReportHistory(report, 4);

            List<RehabStrengthMetrics> strengthHistory = new List<RehabStrengthMetrics>();
            List<RehabReport> strengthReports = new List<RehabReport>();
            List<float[]> strengthSeries = new List<float[]>();
            foreach (RehabReport item in history)
            {
                RehabStrengthMetrics strength = GetStrengthMetrics(item);
                if (strength == null || !strength.HasData) continue;
                strengthHistory.Add(strength);
                strengthReports.Add(item);
                strengthSeries.Add(new[] { strength.Flexion, strength.Extension, strength.Up, strength.Right, strength.Left, strength.Down });
            }
            for (int i = 0; i < 4; i++)
            {
                if (strengthLegendDateTexts != null && strengthLegendDateTexts[i] != null)
                    strengthLegendDateTexts[i].text = i < strengthReports.Count ? strengthReports[i].CreateTime.ToString("yyyy-MM-dd") : "--";
            }
            if (strengthChart != null)
            {
                if (strengthSeries.Count > 0) strengthChart.SetStrengthData(strengthSeries.ToArray());
                else strengthChart.ClearData();
            }
            BindStrengthTable(strengthHistory, strengthReports);
            if (radarSummaryText != null) radarSummaryText.text = BuildStrengthSummary(strengthHistory);
            BindBaselineChart(history);
        }

        private void BindRomCards(List<RehabReport> history)
        {
            for (int i = 0; i < MaxRomCards; i++)
            {
                RehabRomMetrics rom = i < history.Count ? GetRomMetrics(history[i]) : null;
                if (romMetricTexts != null && romMetricTexts[i] != null)
                    romMetricTexts[i].text = rom != null && rom.HasData
                        ? string.Format("宽度：{0:F2} cm　高度：{1:F2} cm　深度：{2:F2} cm", rom.WidthCm, rom.HeightCm, rom.DepthCm)
                        : "宽度：--　高度：--　深度：--";
                if (romVolumeTexts != null && romVolumeTexts[i] != null)
                    romVolumeTexts[i].text = rom != null && rom.HasData && rom.HasVolume
                        ? string.Format("体积：{0:F2} cm³", rom.VolumeCc)
                        : "体积：--";
                if (romDateTexts != null && romDateTexts[i] != null)
                    romDateTexts[i].text = i < history.Count ? history[i].CreateTime.ToString("yyyy-MM-dd") : "--";
                if (romCharts != null && romCharts[i] != null)
                {
                    if (rom != null && rom.HasData)
                    {
                        List<Vector3> points = i < history.Count ? GetRomPoints(history[i]) : null;
                        romCharts[i].SetRomData(rom.WidthCm, rom.HeightCm, rom.DepthCm, points);
                    }
                    else romCharts[i].ClearData();
                    PositionRomAxisLabels(i);
                }
            }
            LayoutRomReport(history.Count);
        }

        private void PositionRomAxisLabels(int index)
        {
            if (romCharts == null || romCharts[index] == null || romAxisLabels == null || romAxisLabels[index] == null) return;
            Vector2 vertical;
            Vector2 coronal;
            Vector2 sagittal;
            if (!romCharts[index].TryGetRomAxisLabelPositions(out vertical, out coronal, out sagittal)) return;
            Vector2[] positions = { vertical, coronal, sagittal };
            for (int i = 0; i < positions.Length; i++)
            {
                TMP_Text label = romAxisLabels[index][i];
                if (label == null) continue;
                RectTransform rect = label.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(positions[i].x, -positions[i].y);
            }
        }

        private void LayoutRomReport(int count)
        {
            int visibleCount = Mathf.Clamp(count, 0, MaxRomCards);
            int rowCount = (visibleCount + 1) / 2;
            if (romCards != null)
            {
                for (int i = 0; i < romCards.Length; i++)
                {
                    if (romCards[i] == null) continue;
                    romCards[i].gameObject.SetActive(i < visibleCount);
                    romCards[i].anchoredPosition = i < visibleCount
                        ? new Vector2(romCardBasePositions[i % 2].x, romCardBasePositions[0].y - (i / 2) * RomCardStepY)
                        : romCardBasePositions[i];
                }
            }

            int removedRows = 2 - rowCount;
            float offset = removedRows * RomCardStepY;
            if (reportFlowNodes != null)
            {
                for (int i = 0; i < reportFlowNodes.Length; i++)
                    reportFlowNodes[i].anchoredPosition = reportFlowBasePositions[i] + new Vector2(0f, offset);
            }

            float baseHeight = reportContentBaseHeight > 1f ? reportContentBaseHeight : 2380f;
            float height = Mathf.Max(1f, baseHeight - offset);
            if (reportContentRect != null) reportContentRect.sizeDelta = new Vector2(reportContentRect.sizeDelta.x, height);
            RectTransform instanceRect = assessmentReportInstance != null ? assessmentReportInstance.GetComponent<RectTransform>() : null;
            if (instanceRect != null) instanceRect.sizeDelta = new Vector2(instanceRect.sizeDelta.x, height);
            if (assessmentContent != null)
                assessmentContent.sizeDelta = new Vector2(assessmentContent.sizeDelta.x, height * AssessmentScale);

            if (romSectionHintText != null)
                romSectionHintText.text = visibleCount == 0
                    ? "暂无关节活动范围评估数据"
                    : string.Format("最近 {0} 次评估 · 最多显示 4 次", visibleCount);
        }

        private List<RehabReport> GetRecentAssessmentHistory()
        {
            List<RehabReport> history = new List<RehabReport>();
            if (reports == null) return history;
            foreach (RehabReport item in reports)
            {
                if (item.Kind == RehabReportKind.Assessment) history.Add(item);
            }
            history.Sort((a, b) =>
            {
                int timeCompare = b.CreateTime.CompareTo(a.CreateTime);
                return timeCompare != 0 ? timeCompare : b.ReportID.CompareTo(a.ReportID);
            });
            if (history.Count > MaxRomCards) history.RemoveRange(MaxRomCards, history.Count - MaxRomCards);
            history.Reverse();
            return history;
        }

        private List<RehabReport> GetReportHistory(RehabReport selected, int limit)
        {
            List<RehabReport> history = new List<RehabReport>();
            foreach (RehabReport item in reports)
            {
                if (item.Kind != selected.Kind || item.CreateTime > selected.CreateTime ||
                    (item.CreateTime == selected.CreateTime && item.ReportID > selected.ReportID)) continue;
                history.Add(item);
                if (history.Count >= limit) break;
            }
            history.Reverse();
            return history;
        }

        private void BindStrengthTable(List<RehabStrengthMetrics> strengthHistory, List<RehabReport> strengthReports)
        {
            int start = Mathf.Max(0, strengthHistory.Count - 3);
            for (int row = 0; row < 3; row++)
            {
                RehabStrengthMetrics strength = start + row < strengthHistory.Count ? strengthHistory[start + row] : null;
                if (strengthRowDateTexts != null && strengthRowDateTexts[row] != null)
                    strengthRowDateTexts[row].text = strength != null ? strengthReports[start + row].CreateTime.ToString("yyyy-MM-dd") : "--";
                float[] values = strength != null
                    ? new[] { strength.Flexion, strength.Extension, strength.Up, strength.Down, strength.Right, strength.Left }
                    : new float[6];
                for (int column = 0; column < 6; column++)
                {
                    if (strengthValueTexts == null || strengthValueTexts[row] == null || strengthValueTexts[row][column] == null) continue;
                    strengthValueTexts[row][column].text = strength != null ? values[column].ToString("F2") : "--";
                }
            }
        }

        private void BindBaselineChart(List<RehabReport> history)
        {
            List<float> participation = new List<float>();
            List<float> trackingError = new List<float>();
            List<float> deviation = new List<float>();
            List<RehabReport> baselineReports = new List<RehabReport>();
            foreach (RehabReport item in history)
            {
                RehabBaselineMetrics baseline = GetBaselineMetrics(item);
                if (baseline == null || !baseline.HasData || baseline.Trajectory == null || !baseline.Trajectory.HasData) continue;
                baselineReports.Add(item);
                participation.Add(baseline.ActiveParticipationPercent);
                trackingError.Add(baseline.Trajectory.TrajectoryTrackingErrorMm);
                deviation.Add(baseline.Trajectory.TrajectoryDeviationMm);
            }
            for (int i = 0; i < 4; i++)
            {
                if (baselineDateTexts != null && baselineDateTexts[i] != null)
                    baselineDateTexts[i].text = i < baselineReports.Count ? baselineReports[i].CreateTime.ToString("yyyy-MM-dd") : "--";
            }
            if (baselineChart != null)
            {
                if (baselineReports.Count > 0)
                    baselineChart.SetBaselineData(participation.ToArray(), trackingError.ToArray(), deviation.ToArray());
                else baselineChart.ClearData();
            }
        }

        private static string BuildStrengthSummary(List<RehabStrengthMetrics> history)
        {
            if (history.Count < 2) return "暂无足够数据生成趋势结论。";
            float[] first = GetStrengthValues(history[0]);
            float[] last = GetStrengthValues(history[history.Count - 1]);
            string[] names = { "矢状轴向前", "矢状轴向后", "垂直轴向上", "垂直轴向下", "冠状轴向右", "冠状轴向左" };
            int bestIndex = 0;
            float bestDelta = float.NegativeInfinity;
            for (int i = 0; i < first.Length; i++)
            {
                float delta = last[i] - first[i];
                if (delta <= bestDelta) continue;
                bestDelta = delta;
                bestIndex = i;
            }
            return bestDelta > 0.0001f
                ? string.Format("较首次评估，{0}提升明显。", names[bestIndex])
                : "六轴肌力较首次评估未见明显提升。";
        }

        private static float[] GetStrengthValues(RehabStrengthMetrics strength)
        {
            return new[] { strength.Flexion, strength.Extension, strength.Up, strength.Down, strength.Right, strength.Left };
        }

        private List<Vector3> GetRomPoints(RehabReport report)
        {
            if (report == null) return null;
            List<Vector3> cached;
            if (romPointCache.TryGetValue(report.ReportID, out cached)) return cached;
            List<RehabReportSample> samples = DBManagerModule.Instance.QueryRehabReportSamples(report.ReportID);
            List<Vector3> points = null;
            if (samples != null && samples.Count >= 4)
            {
                points = new List<Vector3>(samples.Count);
                foreach (RehabReportSample sample in samples)
                {
                    if (sample == null) continue;
                    points.Add(new Vector3(sample.PositionX, sample.PositionZ, sample.PositionY) * 100f);
                }
            }
            romPointCache[report.ReportID] = points;
            return points;
        }

        private static RehabRomMetrics GetRomMetrics(RehabReport report)
        {
            AssessmentReportPayload assessment = GetAssessmentPayload(report);
            if (assessment != null && assessment.Rom != null) return assessment.Rom;
            TrainingReportPayload training = GetTrainingPayload(report);
            return training != null ? training.Rom : null;
        }

        private static RehabStrengthMetrics GetStrengthMetrics(RehabReport report)
        {
            AssessmentReportPayload assessment = GetAssessmentPayload(report);
            if (assessment != null && assessment.Strength != null) return assessment.Strength;
            TrainingReportPayload training = GetTrainingPayload(report);
            return training != null ? training.Strength : null;
        }

        private static RehabBaselineMetrics GetBaselineMetrics(RehabReport report)
        {
            AssessmentReportPayload assessment = GetAssessmentPayload(report);
            if (assessment != null && assessment.Baseline != null) return assessment.Baseline;
            TrainingReportPayload training = GetTrainingPayload(report);
            return training != null ? training.Baseline : null;
        }

        private static AssessmentReportPayload GetAssessmentPayload(RehabReport report)
        {
            if (report == null || report.Kind != RehabReportKind.Assessment || string.IsNullOrEmpty(report.PayloadJson)) return null;
            try { return JsonUtility.FromJson<AssessmentReportPayload>(report.PayloadJson); }
            catch { return null; }
        }

        private static TrainingReportPayload GetTrainingPayload(RehabReport report)
        {
            if (report == null || report.Kind != RehabReportKind.Training || string.IsNullOrEmpty(report.PayloadJson)) return null;
            try { return JsonUtility.FromJson<TrainingReportPayload>(report.PayloadJson); }
            catch { return null; }
        }

        private void HighlightSelectedRow()
        {
            for (int i = 0; i < rowsContent.childCount; i++)
            {
                Transform child = rowsContent.GetChild(i);
                if (child == rowTemplate.transform) continue;
                Image image = child.GetComponent<Image>();
                if (image != null) image.color = child.gameObject == selectedRow ? SelectedBackground : Color.white;
            }
        }

        private void TurnPage(int delta)
        {
            curPage += delta;
            RenderRows(false);
        }

        private void OnPageSelected(int page)
        {
            curPage = page;
            RenderRows(false);
        }

        private void ClearRows()
        {
            for (int i = rowsContent.childCount - 1; i >= 0; i--)
            {
                Transform child = rowsContent.GetChild(i);
                if (child == rowTemplate.transform) continue;
                child.gameObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            selectedRow = null;
        }

        private GameObject FindRowByIndex(int index)
        {
            Transform row = rowsContent.Find("ReportRow_" + index);
            return row != null ? row.gameObject : null;
        }

        private static string GetMetricText(RehabReport report)
        {
            if (report.Kind == RehabReportKind.Training)
            {
                TrainingReportPayload payload = GetTrainingPayload(report);
                if (payload != null && payload.Rom != null && payload.Rom.HasData)
                    return string.Format("训练 · 宽 {0:F1} × 高 {1:F1} × 深 {2:F1} cm", payload.Rom.WidthCm, payload.Rom.HeightCm, payload.Rom.DepthCm);
                return "训练会话归档 · REP-" + report.ReportID.ToString("000000");
            }
            AssessmentReportPayload assessmentPayload = GetAssessmentPayload(report);
            if (assessmentPayload == null) return "ROM 评估 · REP-" + report.ReportID.ToString("000000");
            if (assessmentPayload.Rom != null && assessmentPayload.Rom.HasData)
                return string.Format("{0} · 宽 {1:F1} × 高 {2:F1} × 深 {3:F1} cm", string.IsNullOrEmpty(assessmentPayload.Mode) ? "ROM" : assessmentPayload.Mode, assessmentPayload.Rom.WidthCm, assessmentPayload.Rom.HeightCm, assessmentPayload.Rom.DepthCm);
            return (string.IsNullOrEmpty(assessmentPayload.Mode) ? "ROM" : assessmentPayload.Mode) + " · " +
                assessmentPayload.MinAngle.ToString("F1") + "° ~ " + assessmentPayload.MaxAngle.ToString("F1") + "°";
        }

        private static string GetJointName(AssessmentJoint joint)
        {
            if (joint == AssessmentJoint.Elbow) return "肘关节";
            if (joint == AssessmentJoint.Forearm) return "前臂旋转";
            return "肩关节";
        }

        private static string GetSideName(AffectedSide side)
        {
            if (side == AffectedSide.RIGHT) return "右侧";
            if (side == AffectedSide.LEFTRIGHT) return "双侧";
            return "左侧";
        }

        private static TMP_Text FindText(Transform parent, string childName)
        {
            Transform node = parent != null ? parent.Find(childName) : null;
            return node != null ? node.GetComponent<TMP_Text>() : null;
        }

        private static ReportChartGraphic FindChart(Transform parent, string childName)
        {
            Transform node = parent != null ? parent.Find(childName) : null;
            return node != null ? node.GetComponent<ReportChartGraphic>() : null;
        }

        private static RomRangeChart FindRomChart(Transform parent, string childName)
        {
            Transform node = parent != null ? parent.Find(childName) : null;
            return node != null ? node.GetComponent<RomRangeChart>() : null;
        }

        private static void SetChildText(GameObject parent, string childName, string value)
        {
            TMP_Text text = FindText(parent.transform, childName);
            if (text != null) text.text = value ?? "";
        }

        private static void SetRowTypeColor(GameObject row, RehabReportKind kind)
        {
            Transform badge = row.transform.Find("TypeBadge");
            if (badge == null) return;
            Image image = badge.GetComponent<Image>();
            TMP_Text label = badge.GetComponentInChildren<TMP_Text>();
            if (kind == RehabReportKind.Training)
            {
                if (image != null) image.color = new Color(0.937f, 0.965f, 1f, 1f);
                if (label != null) label.color = new Color(0.114f, 0.306f, 0.847f, 1f);
            }
            else
            {
                if (image != null) image.color = new Color(0.992f, 0.949f, 0.973f, 1f);
                if (label != null) label.color = new Color(0.745f, 0.094f, 0.365f, 1f);
            }
        }

        private static void SetSelected(Button button, bool selected)
        {
            Image image = button.GetComponent<Image>();
            if (image != null) image.color = selected ? SelectedBackground : NormalTabBackground;
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

        public override void OnDestroy()
        {
            allFilterButton.onClick.RemoveAllListeners();
            trainingFilterButton.onClick.RemoveAllListeners();
            assessmentFilterButton.onClick.RemoveAllListeners();
            refreshButton.onClick.RemoveAllListeners();
            prevPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.RemoveAllListeners();
            if (assessmentReportInstance != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(assessmentReportInstance);
                else UnityEngine.Object.DestroyImmediate(assessmentReportInstance);
            }
            base.OnDestroy();
        }
    }
}
