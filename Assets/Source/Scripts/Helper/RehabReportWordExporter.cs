using System;
using System.Collections.Generic;
using System.IO;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using UnityEngine;

namespace LB
{
    public static class RehabReportWordExporter
    {
        private const string FontName = "Microsoft YaHei";

        public static void Export(ReportDocumentModel model, string path)
        {
            if (model == null || model.Report == null) throw new ArgumentNullException("model");
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path");

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            using (XWPFDocument document = new XWPFDocument())
            {
                ConfigurePage(document);
                AppendTitle(document, model);
                AppendMeta(document, model);

                byte[] romImage = ReportChartImageFactory.CreateRomComposite(model);
                byte[] strengthImage = ReportChartImageFactory.CreateStrengthRadar(model);
                byte[] baselineImage = ReportChartImageFactory.CreateBaselineLine(model);

                if (model.Report.Kind == RehabReportKind.Assessment)
                    AppendAssessment(document, model, romImage, strengthImage, baselineImage);
                else
                    AppendTraining(document, model, romImage, strengthImage, baselineImage);

                AppendNotes(document, model, model.Report.Kind == RehabReportKind.Assessment ? "5. 报告说明与备注" : "3. 报告说明与备注");
                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    document.Write(stream);
                }
            }
        }

        private static void ConfigurePage(XWPFDocument document)
        {
            CT_SectPr sectPr = document.Document.body.sectPr;
            if (sectPr == null) sectPr = document.Document.body.AddNewSectPr();
            sectPr.pgSz.w = 11906;
            sectPr.pgSz.h = 16838;
            sectPr.pgMar.top = 850;
            sectPr.pgMar.bottom = 850;
            sectPr.pgMar.left = 850;
            sectPr.pgMar.right = 850;
            sectPr.pgMar.header = 425;
            sectPr.pgMar.footer = 425;
        }

        private static void AppendTitle(XWPFDocument document, ReportDocumentModel model)
        {
            XWPFParagraph title = document.CreateParagraph();
            title.Alignment = ParagraphAlignment.CENTER;
            XWPFRun titleRun = title.CreateRun();
            ConfigureRun(titleRun, 22, true, "1F2937");
            titleRun.SetText(model.Title);

            XWPFParagraph subtitle = document.CreateParagraph();
            subtitle.Alignment = ParagraphAlignment.CENTER;
            XWPFRun subtitleRun = subtitle.CreateRun();
            ConfigureRun(subtitleRun, 9, false, "6B7280");
            subtitleRun.SetText("REHABILITATION REPORT");
        }

        private static void AppendMeta(XWPFDocument document, ReportDocumentModel model)
        {
            PatientInfo patient = model.Patient;
            string[,] rows =
            {
                { "姓名", patient != null ? patient.Name : "--", "病历号", patient != null ? patient.CardNum : "--", "性别", patient != null ? (patient.Sex == Sex.Female ? "女" : "男") : "--" },
                { "年龄", patient != null ? patient.Age.ToString() : "--", "患侧", patient != null ? GetSideName(patient.AffectedSide) : "--", "报告日期", model.Report.CreateTime.ToString("yyyy-MM-dd HH:mm") }
            };
            AddKeyValueTable(document, rows);
        }

        private static void AppendAssessment(XWPFDocument document, ReportDocumentModel model, byte[] romImage, byte[] strengthImage, byte[] baselineImage)
        {
            AssessmentReportPayload payload = model.Assessment;
            AddSectionTitle(document, "1. 基础数据统计");
            AddDataTable(document,
                new[] { "评估模式", "评估轴", "当前角度", "最小角度", "最大角度" },
                new[]
                {
                    new[]
                    {
                        payload != null ? payload.Mode : "--",
                        payload != null ? payload.Axis : "--",
                        payload != null ? payload.CurrentAngle.ToString("F1") + "°" : "--",
                        payload != null ? payload.MinAngle.ToString("F1") + "°" : "--",
                        payload != null ? payload.MaxAngle.ToString("F1") + "°" : "--"
                    }
                });

            AddSectionTitle(document, "2. 关节活动范围评估");
            List<string[]> romRows = new List<string[]>();
            foreach (ReportHistoryItem item in model.History)
            {
                RehabRomMetrics rom = item.Rom;
                bool has = rom != null && rom.HasData;
                romRows.Add(new[]
                {
                    item.Report.CreateTime.ToString("yyyy-MM-dd"),
                    item.Assessment != null ? item.Assessment.Mode : "--",
                    has ? rom.WidthCm.ToString("F2") : "--",
                    has ? rom.HeightCm.ToString("F2") : "--",
                    has ? rom.DepthCm.ToString("F2") : "--",
                    has && rom.HasVolume ? rom.VolumeCc.ToString("F2") : "--"
                });
            }
            AddDataTable(document, new[] { "日期", "模式", "宽度(cm)", "高度(cm)", "深度(cm)", "体积(cm³)" }, romRows);
            AddImage(document, romImage, 620);

            AddSectionTitle(document, "3. 肌力评估");
            List<string[]> strengthRows = new List<string[]>();
            foreach (ReportHistoryItem item in model.History)
            {
                RehabStrengthMetrics strength = item.Strength;
                bool has = strength != null && strength.HasData;
                strengthRows.Add(new[]
                {
                    item.Report.CreateTime.ToString("yyyy-MM-dd"),
                    has ? strength.Flexion.ToString("F2") : "--",
                    has ? strength.Extension.ToString("F2") : "--",
                    has ? strength.Up.ToString("F2") : "--",
                    has ? strength.Down.ToString("F2") : "--",
                    has ? strength.Right.ToString("F2") : "--",
                    has ? strength.Left.ToString("F2") : "--"
                });
            }
            AddDataTable(document, new[] { "日期", "矢状轴向前", "矢状轴向后", "垂直轴上", "垂直轴下", "冠状轴向右", "冠状轴向左" }, strengthRows);
            AddImage(document, strengthImage, 360);

            AddSectionTitle(document, "4. 基准评估");
            List<string[]> baselineRows = new List<string[]>();
            foreach (ReportHistoryItem item in model.History)
            {
                RehabBaselineMetrics baseline = item.Baseline;
                bool has = baseline != null && baseline.HasData && baseline.Trajectory != null && baseline.Trajectory.HasData;
                baselineRows.Add(new[]
                {
                    item.Report.CreateTime.ToString("yyyy-MM-dd"),
                    has ? baseline.ActiveParticipationPercent.ToString("F1") : "--",
                    has ? baseline.Trajectory.TrajectoryTrackingErrorMm.ToString("F1") : "--",
                    has ? baseline.Trajectory.TrajectoryDeviationMm.ToString("F1") : "--"
                });
            }
            AddDataTable(document, new[] { "日期", "主动参与度(%)", "轨迹跟踪误差(mm)", "轨迹偏离度(mm)" }, baselineRows);
            AddImage(document, baselineImage, 620);
        }

        private static void AppendTraining(XWPFDocument document, ReportDocumentModel model, byte[] romImage, byte[] strengthImage, byte[] baselineImage)
        {
            TrainingReportPayload payload = model.Training;
            AddSectionTitle(document, "1. 训练参数");
            AddKeyValueTable(document, new string[,]
            {
                { "训练模式", payload != null ? TrainingCatalog.GetModeName(TrainingCatalog.ParseMode(payload.Mode)) : "--", "运动方向", payload != null ? TrainingCatalog.GetDirectionNames(TrainingCatalog.ParseDirections(payload.Direction)) : "--" },
                { "阻力模型", payload != null ? payload.ResistanceModel : "--", "阻力等级", payload != null ? payload.ResistanceLevel.ToString() : "--" },
                { "训练时长", payload != null ? FormatDuration(payload.DurationSeconds) : "--", "患侧", model.Patient != null ? GetSideName(model.Patient.AffectedSide) : "--" },
                { "限定方向", payload != null && payload.HasDirectionRestriction ? (payload.IsDirectionRestricted ? "是" : "否") : "--", "减重", payload != null && payload.HasWeightReduction ? payload.WeightReductionValue.ToString("F2") + " " + payload.WeightReductionUnit : "--" },
                { "能量", payload != null && payload.HasEnergy ? payload.EnergyValue.ToString("F2") + " " + payload.EnergyUnit : "--", "成功率", payload != null && payload.HasSuccessRate ? payload.SuccessRatePercent.ToString("F1") + " %" : "--" }
            });

            AddSectionTitle(document, "2. 运动指标");
            RehabMovementMetrics movement = payload != null ? payload.Movement : null;
            RehabRomMetrics rom = payload != null ? payload.Rom : null;
            AddDataTable(document,
                new[] { "运动距离(m)", "最大速度(m/s)", "平均速度(m/s)", "ROM 宽×高×深(cm)", "ROM 体积(cm³)" },
                new[]
                {
                    new[]
                    {
                        movement != null && movement.HasData ? movement.DistanceMeters.ToString("F2") : "--",
                        movement != null && movement.HasData ? movement.MaxSpeedMetersPerSecond.ToString("F2") : "--",
                        movement != null && movement.HasData ? movement.AverageSpeedMetersPerSecond.ToString("F2") : "--",
                        rom != null && rom.HasData ? rom.WidthCm.ToString("F2") + " × " + rom.HeightCm.ToString("F2") + " × " + rom.DepthCm.ToString("F2") : "--",
                        rom != null && rom.HasData && rom.HasVolume ? rom.VolumeCc.ToString("F2") : "--"
                    }
                });
            AddImage(document, romImage, 620);

            if (strengthImage != null || baselineImage != null)
            {
                AddSectionTitle(document, "3. 历史评估趋势");
                AddImage(document, strengthImage, 360);
                AddImage(document, baselineImage, 620);
            }
        }

        private static void AppendNotes(XWPFDocument document, ReportDocumentModel model, string title)
        {
            AddSectionTitle(document, title);
            XWPFParagraph paragraph = document.CreateParagraph();
            XWPFRun run = paragraph.CreateRun();
            ConfigureRun(run, 10, false, "4B5563");
            run.SetText("本报告为用户本次康复训练或评估的数据记录，仅用于本院同一设备康复训练系统治疗前后效果比对，对用户训练前后肌力及关节活动状况提供可参考的数据。");
            if (!string.IsNullOrEmpty(model.Note))
            {
                run.AddBreak();
                run.SetText("备注：" + model.Note);
            }
        }

        private static void AddSectionTitle(XWPFDocument document, string text)
        {
            XWPFParagraph paragraph = document.CreateParagraph();
            paragraph.SpacingBefore = 240;
            paragraph.SpacingAfter = 120;
            XWPFRun run = paragraph.CreateRun();
            ConfigureRun(run, 14, true, "1F2937");
            run.SetText(text);
        }

        private static void AddKeyValueTable(XWPFDocument document, string[,] rows)
        {
            int rowCount = rows.GetLength(0);
            int columnCount = rows.GetLength(1);
            XWPFTable table = document.CreateTable(rowCount, columnCount);
            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    XWPFTableCell cell = table.GetRow(row).GetCell(column);
                    bool isLabel = column % 2 == 0;
                    SetCellText(cell, rows[row, column], isLabel);
                    if (isLabel) cell.SetColor("EEF4FB");
                }
            }
        }

        private static void AddDataTable(XWPFDocument document, string[] headers, IEnumerable<string[]> rows)
        {
            List<string[]> rowList = new List<string[]>(rows);
            XWPFTable table = document.CreateTable(rowList.Count + 1, headers.Length);
            for (int column = 0; column < headers.Length; column++)
            {
                XWPFTableCell cell = table.GetRow(0).GetCell(column);
                SetCellText(cell, headers[column], true);
                cell.SetColor("EAF2FB");
            }
            for (int row = 0; row < rowList.Count; row++)
            {
                string[] values = rowList[row];
                for (int column = 0; column < headers.Length; column++)
                    SetCellText(table.GetRow(row + 1).GetCell(column), column < values.Length ? values[column] : "--", false);
            }
        }

        private static void SetCellText(XWPFTableCell cell, string text, bool bold)
        {
            XWPFParagraph paragraph = cell.Paragraphs[0];
            paragraph.Alignment = ParagraphAlignment.CENTER;
            XWPFRun run = paragraph.CreateRun();
            ConfigureRun(run, 9, bold, "1F2937");
            run.SetText(text ?? "--");
        }

        private static void AddImage(XWPFDocument document, byte[] png, int widthPixels)
        {
            if (png == null || png.Length == 0) return;
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(png))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                return;
            }
            int heightPixels = Mathf.Max(1, Mathf.RoundToInt(widthPixels * (float)texture.height / texture.width));
            UnityEngine.Object.DestroyImmediate(texture);

            XWPFParagraph paragraph = document.CreateParagraph();
            paragraph.Alignment = ParagraphAlignment.CENTER;
            XWPFRun run = paragraph.CreateRun();
            using (MemoryStream stream = new MemoryStream(png))
            {
                run.AddPicture(stream, (int)NPOI.SS.UserModel.PictureType.PNG, "report-chart", Units.PixelToEMU(widthPixels), Units.PixelToEMU(heightPixels));
            }
        }

        private static void ConfigureRun(XWPFRun run, int size, bool bold, string color)
        {
            run.FontFamily = FontName;
            run.FontSize = size;
            run.IsBold = bold;
            run.SetColor(color);
        }

        private static string FormatDuration(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int hours = total / 3600;
            int minutes = total / 60 % 60;
            int secs = total % 60;
            return hours > 0 ? hours + " 小时 " + minutes + " 分" : minutes + " 分 " + secs + " 秒";
        }

        private static string GetSideName(AffectedSide side)
        {
            if (side == AffectedSide.RIGHT) return "右侧";
            if (side == AffectedSide.LEFTRIGHT) return "双侧";
            return "左侧";
        }
    }
}
