using System;
using SimpleSQL;

namespace LB
{
    public enum RehabReportKind
    {
        Training = 0,
        Assessment = 1
    }

    public enum AssessmentJoint
    {
        Shoulder = 0,
        Elbow = 1,
        Forearm = 2
    }

    [Serializable]
    public class RehabRomMetrics
    {
        public float WidthCm;
        public float HeightCm;
        public float DepthCm;
        public float VolumeCc;
        public bool HasData;
        public bool HasVolume;
    }

    [Serializable]
    public class RehabStrengthMetrics
    {
        public float Flexion;
        public float Extension;
        public float Up;
        public float Down;
        public float Right;
        public float Left;
        public string Unit;
        public bool HasData;
    }

    [Serializable]
    public class RehabTrajectoryMetrics
    {
        public float TrajectoryDeviationMm;
        public float TrajectoryTrackingErrorMm;
        public bool HasData;
    }

    [Serializable]
    public class RehabBaselineMetrics
    {
        public float ActiveParticipationPercent;
        public RehabTrajectoryMetrics Trajectory;
        public bool HasData;
    }

    [Serializable]
    public class AssessmentReportPayload
    {
        public string Mode;
        public string Axis;
        public float CurrentAngle;
        public float MinAngle;
        public float MaxAngle;
        public RehabRomMetrics Rom;
        public RehabStrengthMetrics Strength;
        public RehabBaselineMetrics Baseline;
        public string Note;
    }

    [Serializable]
    public class TrainingReportPayload
    {
        public string Mode;
        public string Direction;
        public string ResistanceModel;
        public int ResistanceLevel;
        public float DurationSeconds;
        public int SampleCount;
        public RehabRomMetrics Rom;
        public RehabStrengthMetrics Strength;
        public RehabBaselineMetrics Baseline;
        public string Note;
    }

    [Serializable]
    public class RehabReportSample
    {
        [PrimaryKey, AutoIncrement]
        public int SampleID { get; set; }
        [Indexed]
        public int ReportID { get; set; }
        public int SampleIndex { get; set; }
        public double TimestampSeconds { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float JointAngleX { get; set; }
        public float JointAngleY { get; set; }
        public float JointAngleZ { get; set; }
        public float ForceX { get; set; }
        public float ForceY { get; set; }
        public float ForceZ { get; set; }
        public float TorqueX { get; set; }
        public float TorqueY { get; set; }
        public float TorqueZ { get; set; }
        public float CurrentAngleDegrees { get; set; }
        public float SpeedMetersPerSecond { get; set; }
    }

    public class RehabReport
    {
        [PrimaryKey, AutoIncrement]
        public int ReportID { get; set; }

        [MaxLength(64), NotNull]
        public string Title { get; set; }

        [MaxLength(32), NotNull]
        public string PatientID { get; set; }

        [NotNull]
        public RehabReportKind Kind { get; set; }

        [NotNull]
        public AssessmentJoint Joint { get; set; }

        [MaxLength(8192), NotNull]
        public string PayloadJson { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
