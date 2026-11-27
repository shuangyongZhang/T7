using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LB
{
    public static class ReportHtmlBuilder
    {
        public static string Build(ReportDocumentModel model)
        {
            if (model == null || model.Report == null) return string.Empty;
            StringBuilder html = new StringBuilder();
            html.Append("<!doctype html><html><head><meta charset=\"utf-8\"><style>");
            html.Append("@page{size:A4;margin:12mm;}*{box-sizing:border-box;}body{margin:0;font-family:'Microsoft YaHei','Noto Sans CJK SC',sans-serif;color:#1f2937;font-size:12px;line-height:1.5;}h1{font-size:24px;text-align:center;margin:0 0 4px;}h2{font-size:16px;margin:18px 0 8px;padding-left:8px;border-left:4px solid #2f74c0;}h3{font-size:13px;margin:12px 0 6px;}.sub{text-align:center;color:#6b7280;font-size:10px;letter-spacing:1px;margin-bottom:16px;}.meta{width:100%;border-collapse:collapse;margin:8px 0 14px;}.meta td{border:1px solid #cfd8e3;padding:6px 8px;}.meta .label{width:12%;background:#f3f6fa;color:#4b5563;}.chart{width:100%;border:1px solid #d8dee8;padding:8px;margin:8px 0;text-align:center;}.chart img{max-width:100%;height:auto;}table.data{width:100%;border-collapse:collapse;margin:6px 0 12px;table-layout:fixed;}table.data th,table.data td{border:1px solid #cfd8e3;padding:5px 4px;text-align:center;word-break:break-all;}table.data th{background:#eef4fb;color:#374151;font-weight:600;}.note{border:1px solid #d8dee8;background:#fafbfd;padding:10px;white-space:pre-wrap;}.footer{margin-top:18px;padding-top:8px;border-top:1px solid #d8dee8;color:#6b7280;font-size:10px;display:flex;justify-content:space-between;}.section{page-break-inside:avoid;}.muted{color:#6b7280;}");
            html.Append("</style></head><body>");
            html.Append("<h1>").Append(Escape(model.Title)).Append("</h1>");
            html.Append("<div class=\"sub\">REHABILITATION REPORT</div>");
            AppendMeta(html, model);

            byte[] romImage = ReportChartImageFactory.CreateRomComposite(model);
            byte[] strengthImage = ReportChartImageFactory.CreateStrengthRadar(model);
            byte[] baselineImage = ReportChartImageFactory.CreateBaselineLine(model);

            if (model.Report.Kind == RehabReportKind.Assessment)
                AppendAssessment(html, model, romImage, strengthImage, baselineImage);
            else
                AppendTraining(html, model, romImage, strengthImage, baselineImage);

            html.Append("<div class=\"footer\"><span>报告编号：").Append(Escape(model.ReportNumber)).Append("</span><span>康复训练系统 · 数据仅供临床参考</span></div>");
            html.Append("</body></html>");
            return html.ToString();
        }

        private static void AppendMeta(StringBuilder html, ReportDocumentModel model)
        {
            PatientInfo patient = model.Patient;
            html.Append("<table class=\"meta\">");
            html.Append("<tr><td class=\"label\">姓名</td><td>").Append(Escape(patient != null ? patient.Name : "--")).Append("</td>");
            html.Append("<td class=\"label\">病历号</td><td>").Append(Escape(patient != null ? patient.CardNum : "--")).Append("</td>");
            html.Append("<td class=\"label\">性别</td><td>").Append(Escape(patient != null ? (patient.Sex == Sex.Female ? "女" : "男") : "--")).Append("</td></tr>");
            html.Append("<tr><td class=\"label\">年龄</td><td>").Append(patient != null ? patient.Age.ToString() : "--").Append("</td>");
            html.Append("<td class=\"label\">患侧</td><td>").Append(Escape(patient != null ? GetSideName(patient.AffectedSide) : "--")).Append("</td>");
            html.Append("<td class=\"label\">报告日期</td><td>").Append(model.Report.CreateTime.ToString("yyyy-MM-dd HH:mm")).Append("</td></tr>");
            html.Append("</table>");
        }

        private static void AppendAssessment(StringBuilder html, ReportDocumentModel model, byte[] romImage, byte[] strengthImage, byte[] baselineImage)
        {
            AssessmentReportPayload payload = model.Assessment;
            html.Append("<div class=\"section\"><h2>1. 基础数据统计</h2><table class=\"data\">");
            html.Append("<tr><th>评估模式</th><th>评估轴</th><th>当前角度</th><th>最小角度</th><th>最大角度</th></tr>");
            html.Append("<tr><td>").Append(Escape(payload != null ? payload.Mode : "--")).Append("</td><td>").Append(Escape(payload != null ? payload.Axis : "--")).Append("</td><td>").Append(FormatAngle(payload != null ? payload.CurrentAngle : 0f, payload != null)).Append("</td><td>").Append(FormatAngle(payload != null ? payload.MinAngle : 0f, payload != null)).Append("</td><td>").Append(FormatAngle(payload != null ? payload.MaxAngle : 0f, payload != null)).Append("</td></tr>");
            html.Append("</table></div>");

            html.Append("<div class=\"section\"><h2>2. 关节活动范围评估</h2>");
            html.Append("<table class=\"data\"><tr><th>日期</th><th>模式</th><th>宽度(cm)</th><th>高度(cm)</th><th>深度(cm)</th><th>体积(cm³)</th></tr>");
            foreach (ReportHistoryItem item in model.History)
            {
                RehabRomMetrics rom = item.Rom;
                html.Append("<tr><td>").Append(item.Report.CreateTime.ToString("yyyy-MM-dd")).Append("</td><td>").Append(Escape(item.Assessment != null ? item.Assessment.Mode : "--")).Append("</td><td>").Append(FormatNumber(rom != null && rom.HasData ? rom.WidthCm : (float?)null, 2)).Append("</td><td>").Append(FormatNumber(rom != null && rom.HasData ? rom.HeightCm : (float?)null, 2)).Append("</td><td>").Append(FormatNumber(rom != null && rom.HasData ? rom.DepthCm : (float?)null, 2)).Append("</td><td>").Append(FormatNumber(rom != null && rom.HasData && rom.HasVolume ? rom.VolumeCc : (float?)null, 2)).Append("</td></tr>");
            }
            html.Append("</table>");
            if (romImage != null) html.Append("<div class=\"chart\"><img src=\"data:image/png;base64,").Append(Convert.ToBase64String(romImage)).Append("\"></div>");
            html.Append("</div>");

            html.Append("<div class=\"section\"><h2>3. 肌力评估</h2>");
            html.Append("<table class=\"data\"><tr><th>日期</th><th>矢状轴向前</th><th>矢状轴向后</th><th>垂直轴上</th><th>垂直轴下</th><th>冠状轴向右</th><th>冠状轴向左</th></tr>");
            foreach (ReportHistoryItem item in model.History)
            {
                RehabStrengthMetrics strength = item.Strength;
                bool has = strength != null && strength.HasData;
                html.Append("<tr><td>").Append(item.Report.CreateTime.ToString("yyyy-MM-dd")).Append("</td>");
                html.Append("<td>").Append(FormatNumber(has ? strength.Flexion : (float?)null, 2)).Append("</td>");
                html.Append("<td>").Append(FormatNumber(has ? strength.Extension : (float?)null, 2)).Append("</td>");
                html.Append("<td>").Append(FormatNumber(has ? strength.Up : (float?)null, 2)).Append("</td>");
                html.Append("<td>").Append(FormatNumber(has ? strength.Down : (float?)null, 2)).Append("</td>");
                html.Append("<td>").Append(FormatNumber(has ? strength.Right : (float?)null, 2)).Append("</td>");
                html.Append("<td>").Append(FormatNumber(has ? strength.Left : (float?)null, 2)).Append("</td></tr>");
            }
            html.Append("</table>");
            if (strengthImage != null) html.Append("<div class=\"chart\"><img src=\"data:image/png;base64,").Append(Convert.ToBase64String(strengthImage)).Append("\"></div>");
            html.Append("</div>");

            html.Append("<div class=\"section\"><h2>4. 基准评估</h2>");
            html.Append("<table class=\"data\"><tr><th>日期</th><th>主动参与度(%)</th><th>轨迹跟踪误差(mm)</th><th>轨迹偏离度(mm)</th></tr>");
            foreach (ReportHistoryItem item in model.History)
            {
                RehabBaselineMetrics baseline = item.Baseline;
                bool has = baseline != null && baseline.HasData && baseline.Trajectory != null && baseline.Trajectory.HasData;
                html.Append("<tr><td>").Append(item.Report.CreateTime.ToString("yyyy-MM-dd")).Append("</td><td>").Append(FormatNumber(has ? baseline.ActiveParticipationPercent : (float?)null, 1)).Append("</td><td>").Append(FormatNumber(has ? baseline.Trajectory.TrajectoryTrackingErrorMm : (float?)null, 1)).Append("</td><td>").Append(FormatNumber(has ? baseline.Trajectory.TrajectoryDeviationMm : (float?)null, 1)).Append("</td></tr>");
            }
            html.Append("</table>");
            if (baselineImage != null) html.Append("<div class=\"chart\"><img src=\"data:image/png;base64,").Append(Convert.ToBase64String(baselineImage)).Append("\"></div>");
            html.Append("</div>");

            AppendNotes(html, model, "5. 报告说明与备注");
        }

        private static void AppendTraining(StringBuilder html, ReportDocumentModel model, byte[] romImage, byte[] strengthImage, byte[] baselineImage)
        {
            TrainingReportPayload payload = model.Training;
            html.Append("<div class=\"section\"><h2>1. 训练参数</h2><table class=\"data\">");
            html.Append("<tr><th>训练模式</th><th>运动方向</th><th>阻力模型</th><th>阻力等级</th><th>训练时长</th></tr>");
            html.Append("<tr><td>").Append(Escape(payload != null ? TrainingCatalog.GetModeName(TrainingCatalog.ParseMode(payload.Mode)) : "--")).Append("</td><td>").Append(Escape(payload != null ? TrainingCatalog.GetDirectionNames(TrainingCatalog.ParseDirections(payload.Direction)) : "--")).Append("</td><td>").Append(Escape(payload != null ? payload.ResistanceModel : "--")).Append("</td><td>").Append(payload != null ? payload.ResistanceLevel.ToString() : "--").Append("</td><td>").Append(payload != null ? FormatDuration(payload.DurationSeconds) : "--").Append("</td></tr>");
            html.Append("</table><table class=\"data\">");
            html.Append("<tr><th>患侧</th><th>限定方向</th><th>减重</th><th>能量</th><th>成功率</th></tr>");
            html.Append("<tr><td>").Append(Escape(model.Patient != null ? GetSideName(model.Patient.AffectedSide) : "--")).Append("</td><td>").Append(payload != null && payload.HasDirectionRestriction ? (payload.IsDirectionRestricted ? "是" : "否") : "--").Append("</td><td>").Append(payload != null && payload.HasWeightReduction ? payload.WeightReductionValue.ToString("F2") + " " + payload.WeightReductionUnit : "--").Append("</td><td>").Append(payload != null && payload.HasEnergy ? payload.EnergyValue.ToString("F2") + " " + payload.EnergyUnit : "--").Append("</td><td>").Append(payload != null && payload.HasSuccessRate ? payload.SuccessRatePercent.ToString("F1") + " %" : "--").Append("</td></tr>");
            html.Append("</table></div>");

            html.Append("<div class=\"section\"><h2>2. 运动指标</h2><table class=\"data\">");
            html.Append("<tr><th>运动距离(m)</th><th>最大速度(m/s)</th><th>平均速度(m/s)</th><th>ROM 宽×高×深(cm)</th><th>ROM 体积(cm³)</th></tr>");
            RehabMovementMetrics movement = payload != null ? payload.Movement : null;
            RehabRomMetrics rom = payload != null ? payload.Rom : null;
            html.Append("<tr><td>").Append(FormatNumber(movement != null && movement.HasData ? movement.DistanceMeters : (float?)null, 2)).Append("</td><td>").Append(FormatNumber(movement != null && movement.HasData ? movement.MaxSpeedMetersPerSecond : (float?)null, 2)).Append("</td><td>").Append(FormatNumber(movement != null && movement.HasData ? movement.AverageSpeedMetersPerSecond : (float?)null, 2)).Append("</td><td>").Append(rom != null && rom.HasData ? rom.WidthCm.ToString("F2") + " × " + rom.HeightCm.ToString("F2") + " × " + rom.DepthCm.ToString("F2") : "--").Append("</td><td>").Append(FormatNumber(rom != null && rom.HasData && rom.HasVolume ? rom.VolumeCc : (float?)null, 2)).Append("</td></tr>");
            html.Append("</table>");
            if (romImage != null) html.Append("<div class=\"chart\"><img src=\"data:image/png;base64,").Append(Convert.ToBase64String(romImage)).Append("\"></div>");
            html.Append("</div>");

            if (strengthImage != null || baselineImage != null)
            {
                html.Append("<div class=\"section\"><h2>3. 历史评估趋势</h2>");
                if (strengthImage != null) html.Append("<div class=\"chart\"><img src=\"data:image/png;base64,").Append(Convert.ToBase64String(strengthImage)).Append("\"></div>");
                if (baselineImage != null) html.Append("<div class=\"chart\"><img src=\"data:image/png;base64,").Append(Convert.ToBase64String(baselineImage)).Append("\"></div>");
                html.Append("</div>");
            }

            AppendNotes(html, model, "3. 报告说明与备注");
        }

        private static void AppendNotes(StringBuilder html, ReportDocumentModel model, string title)
        {
            html.Append("<div class=\"section\"><h2>").Append(Escape(title)).Append("</h2><div class=\"note\">");
            html.Append("本报告为用户本次康复训练或评估的数据记录，仅用于本院同一设备康复训练系统治疗前后效果比对，对用户训练前后肌力及关节活动状况提供可参考的数据。");
            if (!string.IsNullOrEmpty(model.Note)) html.Append("\n\n备注：").Append(Escape(model.Note));
            html.Append("</div></div>");
        }

        private static string FormatNumber(float? value, int digits)
        {
            return value.HasValue ? value.Value.ToString("F" + digits) : "--";
        }

        private static string FormatAngle(float value, bool hasPayload)
        {
            return hasPayload ? value.ToString("F1") + "°" : "--";
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

        private static string Escape(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : System.Net.WebUtility.HtmlEncode(value);
        }
    }
}
