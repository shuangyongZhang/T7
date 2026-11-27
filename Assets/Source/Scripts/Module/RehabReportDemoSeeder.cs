using System;
using System.Collections.Generic;
using HT.Framework;
using UnityEngine;

namespace LB
{
    public static class RehabReportDemoSeeder
    {
        public const string DemoMarker = "T7_REPORT_DEMO";
        private const int TrainingReportCount = 12;
        private const int AssessmentReportCount = 6;
        private const int SampleCount = 160;

        public static bool GenerateForCurrentPatient()
        {
            if (Global.CurPatientInfo == null)
            {
                Log.Warning("[RehabReportDemoSeeder] 请先选择患者。");
                return false;
            }

            return GenerateForPatient(Global.CurPatientInfo.ID);
        }

        public static bool ClearForCurrentPatient()
        {
            if (Global.CurPatientInfo == null)
            {
                Log.Warning("[RehabReportDemoSeeder] 请先选择患者。");
                return false;
            }

            return ClearForPatient(Global.CurPatientInfo.ID);
        }

        public static bool GenerateForPatient(string patientID)
        {
            if (string.IsNullOrEmpty(patientID)) return false;
            PatientInfo patient = DBManagerModule.Instance.QueryPatientInfoByID(patientID);
            if (patient == null)
            {
                Log.Warning("[RehabReportDemoSeeder] 患者不存在：" + patientID);
                return false;
            }

            ClearForPatient(patientID);

            DateTime firstDate = DateTime.Now.Date.AddDays(-89);
            int sequence = 0;
            for (int i = 0; i < TrainingReportCount; i++)
            {
                DateTime createTime = firstDate.AddDays(i * 7).AddHours(9 + i % 8).AddMinutes(15 * (i % 4));
                if (!InsertTrainingReport(patient, createTime, i, sequence++)) return false;
            }

            for (int i = 0; i < AssessmentReportCount; i++)
            {
                DateTime createTime = firstDate.AddDays(3 + i * 14).AddHours(14 + i % 6).AddMinutes(10 * (i % 5));
                if (!InsertAssessmentReport(patient, createTime, i, sequence++)) return false;
            }

            Log.Info(string.Format("[RehabReportDemoSeeder] 已为患者 {0} 生成 {1} 份训练报告和 {2} 份评估报告。",
                patient.Name, TrainingReportCount, AssessmentReportCount));
            return true;
        }

        public static bool ClearForPatient(string patientID)
        {
            if (string.IsNullOrEmpty(patientID)) return false;
            List<RehabReport> reports = DBManagerModule.Instance.QueryRehabReports(patientID);
            if (reports == null) return true;

            int removed = 0;
            foreach (RehabReport report in reports)
            {
                if (!IsDemoReport(report)) continue;
                if (DBManagerModule.Instance.DeleteRehabReport(report.ReportID)) removed++;
            }

            if (removed > 0) Log.Info("[RehabReportDemoSeeder] 已清理演示报告 " + removed + " 份。");
            return true;
        }

        private static bool IsDemoReport(RehabReport report)
        {
            if (report == null || string.IsNullOrEmpty(report.PayloadJson)) return false;
            if (report.Kind == RehabReportKind.Training)
            {
                TrainingReportPayload payload = JsonUtility.FromJson<TrainingReportPayload>(report.PayloadJson);
                return payload != null && !string.IsNullOrEmpty(payload.Note) && payload.Note.Contains(DemoMarker);
            }

            AssessmentReportPayload assessmentPayload = JsonUtility.FromJson<AssessmentReportPayload>(report.PayloadJson);
            return assessmentPayload != null && !string.IsNullOrEmpty(assessmentPayload.Note) && assessmentPayload.Note.Contains(DemoMarker);
        }

        private static bool InsertTrainingReport(PatientInfo patient, DateTime createTime, int index, int sequence)
        {
            List<RehabSample> samples = CreateSamples(sequence, 0.75f + index * 0.035f);
            RehabRomMetrics rom = RehabMetricsCalculator.CalculateRom(samples);
            RehabMovementMetrics movement = RehabMetricsCalculator.CalculateMovement(samples);
            TrainingMode[] modes = { TrainingMode.Cpm, TrainingMode.Assist, TrainingMode.Active, TrainingMode.Resist, TrainingMode.Teaching, TrainingMode.Intelligent };
            string[][] directionSets =
            {
                new[] { "flexion", "extension" },
                new[] { "abduction", "rotation" },
                new[] { "horizontal", "pronation" },
                new[] { "flexion", "abduction", "rotation" }
            };

            TrainingMode mode = modes[index % modes.Length];
            float durationSeconds = (5 + (index % 6) * 5) * 60f;
            TrainingReportPayload payload = new TrainingReportPayload
            {
                Mode = TrainingCatalog.GetModeReportName(mode),
                Direction = string.Join(",", directionSets[index % directionSets.Length]),
                ResistanceModel = index % 3 == 0 ? "等速自适应" : "弹性助力",
                ResistanceLevel = 1 + index % 10,
                DurationSeconds = durationSeconds,
                SampleCount = samples.Count,
                GameId = "demo-game-" + (index % 4 + 1),
                TreatmentPlanId = 0,
                HasAffectedSide = true,
                AffectedSide = patient.AffectedSide,
                HasDirectionRestriction = true,
                IsDirectionRestricted = index % 2 == 0,
                Movement = movement,
                HasWeightReduction = index % 5 != 4,
                WeightReductionValue = 2f + index * 0.4f,
                WeightReductionUnit = "kg",
                HasEnergy = index % 5 != 4,
                EnergyValue = 12f + index * 1.7f,
                EnergyUnit = "kJ",
                HasSuccessRate = index % 4 != 3,
                SuccessRatePercent = 62f + index * 2.1f,
                Rom = rom,
                Strength = CreateStrength(index * 3 + 1, index % 4 != 3),
                Baseline = CreateBaseline(index * 3 + 1, index % 4 != 2),
                Note = DemoMarker + "；仅用于界面验收。"
            };

            RehabReport report = new RehabReport
            {
                Title = "上肢训练报告（演示）",
                PatientID = patient.ID,
                Kind = RehabReportKind.Training,
                Joint = AssessmentJoint.Shoulder,
                PayloadJson = JsonUtility.ToJson(payload),
                StartTime = createTime.AddSeconds(-durationSeconds),
                EndTime = createTime,
                CreateTime = createTime
            };
            return DBManagerModule.Instance.InsertRehabReport(report, samples);
        }

        private static bool InsertAssessmentReport(PatientInfo patient, DateTime createTime, int index, int sequence)
        {
            List<RehabSample> samples = CreateSamples(sequence, 0.55f + index * 0.06f);
            RehabRomMetrics rom = RehabMetricsCalculator.CalculateRom(samples);
            AssessmentJoint[] joints = { AssessmentJoint.Shoulder, AssessmentJoint.Elbow, AssessmentJoint.Forearm };
            string[] axes = { "垂直轴", "冠状轴", "矢状轴" };
            float minAngle = -35f - index * 3f;
            float maxAngle = 65f + index * 5f;
            AssessmentReportPayload payload = new AssessmentReportPayload
            {
                Mode = index % 2 == 0 ? "AROM" : "PROM",
                Axis = axes[index % axes.Length],
                CurrentAngle = Mathf.Lerp(minAngle, maxAngle, 0.55f),
                MinAngle = minAngle,
                MaxAngle = maxAngle,
                Rom = rom,
                Strength = CreateStrength(index * 2 + 2, index % 3 != 1),
                Baseline = CreateBaseline(index * 2 + 2, index % 3 != 2),
                Note = DemoMarker + "；仅用于界面验收。"
            };

            RehabReport report = new RehabReport
            {
                Title = GetJointName(joints[index % joints.Length]) + " ROM 评估报告（演示）",
                PatientID = patient.ID,
                Kind = RehabReportKind.Assessment,
                Joint = joints[index % joints.Length],
                PayloadJson = JsonUtility.ToJson(payload),
                StartTime = createTime.AddMinutes(-8),
                EndTime = createTime,
                CreateTime = createTime
            };
            return DBManagerModule.Instance.InsertRehabReport(report, samples);
        }

        private static List<RehabSample> CreateSamples(int seed, float scale)
        {
            System.Random random = new System.Random(7301 + seed * 97);
            List<RehabSample> samples = new List<RehabSample>(SampleCount);
            double phase = random.NextDouble() * Math.PI * 2d;
            for (int i = 0; i < SampleCount; i++)
            {
                float t = i / (float)(SampleCount - 1);
                float noiseX = ((float)random.NextDouble() - 0.5f) * 0.012f;
                float noiseY = ((float)random.NextDouble() - 0.5f) * 0.010f;
                float noiseZ = ((float)random.NextDouble() - 0.5f) * 0.010f;
                Vector3 position = new Vector3(
                    (Mathf.Sin(t * Mathf.PI * 4f + (float)phase) * 0.16f + t * 0.05f + noiseX) * scale,
                    (Mathf.Cos(t * Mathf.PI * 3f + (float)phase * 0.7f) * 0.13f + t * 0.03f + noiseY) * scale,
                    (Mathf.Sin(t * Mathf.PI * 5f + (float)phase * 1.3f) * 0.11f + noiseZ) * scale);
                float speed = 0.08f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 6f)) * 0.22f;
                samples.Add(new RehabSample
                {
                    TimestampSeconds = t * 30d,
                    PositionMeters = position,
                    JointAnglesDegrees = new Vector3(-40f + t * 110f, -25f + t * 60f, -15f + t * 45f),
                    ForceNewtons = new Vector3(Mathf.Sin(t * 12f) * 18f, Mathf.Cos(t * 10f) * 15f, Mathf.Sin(t * 8f) * 12f),
                    TorqueNewtonMeters = new Vector3(Mathf.Sin(t * 9f) * 3f, Mathf.Cos(t * 11f) * 2.5f, Mathf.Sin(t * 7f) * 2f),
                    CurrentAngleDegrees = Mathf.Lerp(-35f, 95f, t),
                    SpeedMetersPerSecond = speed
                });
            }
            return samples;
        }

        private static RehabStrengthMetrics CreateStrength(int seed, bool hasData)
        {
            if (!hasData) return new RehabStrengthMetrics { Unit = "N/N·m", HasData = false };
            return new RehabStrengthMetrics
            {
                Flexion = 42f + seed * 1.8f,
                Extension = 35f + seed * 1.4f,
                Up = 48f + seed * 1.6f,
                Down = 31f + seed * 1.2f,
                Right = 39f + seed * 1.5f,
                Left = 34f + seed * 1.3f,
                Unit = "N/N·m",
                HasData = true
            };
        }

        private static RehabBaselineMetrics CreateBaseline(int seed, bool hasData)
        {
            if (!hasData) return new RehabBaselineMetrics { HasData = false };
            return new RehabBaselineMetrics
            {
                ActiveParticipationPercent = 58f + seed * 1.25f,
                Trajectory = new RehabTrajectoryMetrics
                {
                    TrajectoryDeviationMm = 18f - seed * 0.25f,
                    TrajectoryTrackingErrorMm = 12f - seed * 0.18f,
                    HasData = true
                },
                HasData = true
            };
        }

        private static string GetJointName(AssessmentJoint joint)
        {
            if (joint == AssessmentJoint.Elbow) return "肘关节";
            if (joint == AssessmentJoint.Forearm) return "前臂旋转";
            return "肩关节";
        }
    }
}
