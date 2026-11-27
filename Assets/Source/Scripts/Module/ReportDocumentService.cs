using System;
using System.Collections.Generic;
using UnityEngine;

namespace LB
{
    public class ReportHistoryItem
    {
        public RehabReport Report;
        public AssessmentReportPayload Assessment;
        public TrainingReportPayload Training;
        public List<Vector3> RomPoints;

        public RehabRomMetrics Rom
        {
            get { return Assessment != null ? Assessment.Rom : Training != null ? Training.Rom : null; }
        }

        public RehabStrengthMetrics Strength
        {
            get { return Assessment != null ? Assessment.Strength : Training != null ? Training.Strength : null; }
        }

        public RehabBaselineMetrics Baseline
        {
            get { return Assessment != null ? Assessment.Baseline : Training != null ? Training.Baseline : null; }
        }
    }

    public class ReportDocumentModel
    {
        public PatientInfo Patient;
        public RehabReport Report;
        public AssessmentReportPayload Assessment;
        public TrainingReportPayload Training;
        public List<ReportHistoryItem> History = new List<ReportHistoryItem>();

        public string ReportNumber
        {
            get { return "REP-" + Report.ReportID.ToString("000000"); }
        }

        public string Title
        {
            get
            {
                if (Report.Kind == RehabReportKind.Training) return "康复训练报告";
                return "康复评估报告";
            }
        }

        public string Note
        {
            get
            {
                if (Assessment != null) return Assessment.Note;
                if (Training != null) return Training.Note;
                return null;
            }
        }
    }

    public static class ReportDocumentService
    {
        public static ReportDocumentModel Build(RehabReport report)
        {
            if (report == null) return null;
            PatientInfo patient = Global.CurPatientInfo != null && Global.CurPatientInfo.ID == report.PatientID
                ? Global.CurPatientInfo
                : DBManagerModule.Instance.QueryPatientInfoByID(report.PatientID);

            ReportDocumentModel model = new ReportDocumentModel
            {
                Patient = patient,
                Report = report
            };

            if (report.Kind == RehabReportKind.Training)
                model.Training = JsonUtility.FromJson<TrainingReportPayload>(report.PayloadJson);
            else
                model.Assessment = JsonUtility.FromJson<AssessmentReportPayload>(report.PayloadJson);

            List<RehabReport> reports = DBManagerModule.Instance.QueryRehabReports(report.PatientID);
            if (reports == null) return model;
            reports.Sort((a, b) =>
            {
                int result = a.CreateTime.CompareTo(b.CreateTime);
                return result != 0 ? result : a.ReportID.CompareTo(b.ReportID);
            });

            foreach (RehabReport item in reports)
            {
                if (item.Kind != report.Kind) continue;
                if (item.CreateTime > report.CreateTime ||
                    (item.CreateTime == report.CreateTime && item.ReportID > report.ReportID)) continue;

                ReportHistoryItem historyItem = new ReportHistoryItem
                {
                    Report = item,
                    RomPoints = QueryRomPoints(item.ReportID)
                };
                if (item.Kind == RehabReportKind.Training)
                    historyItem.Training = JsonUtility.FromJson<TrainingReportPayload>(item.PayloadJson);
                else
                    historyItem.Assessment = JsonUtility.FromJson<AssessmentReportPayload>(item.PayloadJson);
                model.History.Add(historyItem);
            }

            while (model.History.Count > 4) model.History.RemoveAt(0);
            return model;
        }

        private static List<Vector3> QueryRomPoints(int reportID)
        {
            List<RehabReportSample> samples = DBManagerModule.Instance.QueryRehabReportSamples(reportID);
            if (samples == null || samples.Count < 4) return null;
            List<Vector3> points = new List<Vector3>(samples.Count);
            foreach (RehabReportSample sample in samples)
            {
                if (sample == null) continue;
                points.Add(new Vector3(sample.PositionX, sample.PositionZ, sample.PositionY) * 100f);
            }
            return points;
        }
    }
}
