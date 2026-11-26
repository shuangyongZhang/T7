using System;
using System.Collections.Generic;
using UnityEngine;

namespace LB
{
    [Serializable]
    public sealed class RehabSample
    {
        public double TimestampSeconds;
        public Vector3 PositionMeters;
        public Vector3 JointAnglesDegrees;
        public Vector3 ForceNewtons;
        public Vector3 TorqueNewtonMeters;
        public float CurrentAngleDegrees;
        public float SpeedMetersPerSecond;

        public bool IsValid
        {
            get
            {
                return !double.IsNaN(TimestampSeconds) && !double.IsInfinity(TimestampSeconds) &&
                       IsFinite(PositionMeters) && IsFinite(JointAnglesDegrees) &&
                       IsFinite(ForceNewtons) && IsFinite(TorqueNewtonMeters) &&
                       !float.IsNaN(CurrentAngleDegrees) && !float.IsInfinity(CurrentAngleDegrees) &&
                       !float.IsNaN(SpeedMetersPerSecond) && !float.IsInfinity(SpeedMetersPerSecond);
            }
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                   !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                   !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }
    }

    public static class RehabTelemetryHub
    {
        public static bool IsConnected { get; private set; }
        public static event Action<bool> ConnectionChanged;
        public static event Action<RehabSample> SampleReceived;
        public static event Action<RehabStrengthMetrics, RehabBaselineMetrics> MetricsReceived;

        public static void SetConnected(bool connected)
        {
            if (IsConnected == connected) return;
            IsConnected = connected;
            ConnectionChanged?.Invoke(connected);
        }

        public static void Publish(RehabSample sample)
        {
            if (!IsConnected || sample == null || !sample.IsValid) return;
            SampleReceived?.Invoke(sample);
        }

        public static void PublishMetrics(RehabStrengthMetrics strength, RehabBaselineMetrics baseline)
        {
            if (!IsConnected || (strength == null && baseline == null)) return;
            MetricsReceived?.Invoke(strength, baseline);
        }
    }

    public sealed class RehabSessionRecorder
    {
        private readonly List<RehabSample> samples = new List<RehabSample>();
        private double lastTimestamp = double.NegativeInfinity;

        public int SampleCount => samples.Count;
        public IReadOnlyList<RehabSample> Samples => samples;
        public RehabStrengthMetrics Strength { get; set; }
        public RehabBaselineMetrics Baseline { get; set; }

        public void Reset()
        {
            samples.Clear();
            lastTimestamp = double.NegativeInfinity;
            Strength = null;
            Baseline = null;
        }

        public bool AddSample(RehabSample sample, double minimumIntervalSeconds = 0.05)
        {
            if (sample == null || !sample.IsValid) return false;
            if (samples.Count > 0 && sample.TimestampSeconds - lastTimestamp < minimumIntervalSeconds) return false;
            samples.Add(sample);
            lastTimestamp = sample.TimestampSeconds;
            return true;
        }

        public RehabRomMetrics CalculateRom()
        {
            return RehabMetricsCalculator.CalculateRom(samples);
        }

        public RehabTrajectoryMetrics CalculateTrajectory(IList<Vector3> referenceTrajectory)
        {
            if (referenceTrajectory == null) return null;
            List<Vector3> actual = new List<Vector3>(samples.Count);
            foreach (RehabSample sample in samples) actual.Add(sample.PositionMeters);
            return RehabMetricsCalculator.CalculateTrajectory(actual, referenceTrajectory);
        }
    }

    public static class RehabMetricsCalculator
    {
        public static RehabRomMetrics CalculateRom(IList<RehabSample> samples)
        {
            if (samples == null || samples.Count == 0) return null;

            Vector3 min = samples[0].PositionMeters;
            Vector3 max = min;
            for (int i = 1; i < samples.Count; i++)
            {
                Vector3 point = samples[i].PositionMeters;
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }

            float width = (max.x - min.x) * 100f;
            float height = (max.y - min.y) * 100f;
            float depth = (max.z - min.z) * 100f;
            // T7 使用真实轨迹包围盒体积；BURT 的专有体积算法未公开。
            return new RehabRomMetrics
            {
                WidthCm = width,
                HeightCm = height,
                DepthCm = depth,
                VolumeCc = width * height * depth,
                HasData = true,
                HasVolume = true
            };
        }

        public static RehabTrajectoryMetrics CalculateTrajectory(IList<Vector3> actual, IList<Vector3> reference)
        {
            if (actual == null || reference == null || actual.Count < 2 || reference.Count < 2) return null;

            float[] segmentLengths = new float[reference.Count - 1];
            float[] cumulativeLengths = new float[reference.Count];
            for (int i = 0; i < segmentLengths.Length; i++)
            {
                segmentLengths[i] = Vector3.Distance(reference[i], reference[i + 1]);
                cumulativeLengths[i + 1] = cumulativeLengths[i] + segmentLengths[i];
            }

            float actualLength = 0f;
            float totalDeviation = 0f;
            float totalTrackingError = 0f;
            for (int i = 0; i < actual.Count; i++)
            {
                if (i > 0) actualLength += Vector3.Distance(actual[i - 1], actual[i]);

                float nearestDistance = float.PositiveInfinity;
                float nearestReferenceLength = 0f;
                for (int segment = 0; segment < segmentLengths.Length; segment++)
                {
                    Vector3 start = reference[segment];
                    Vector3 direction = reference[segment + 1] - start;
                    float lengthSquared = direction.sqrMagnitude;
                    float t = lengthSquared > 0.0000001f
                        ? Mathf.Clamp01(Vector3.Dot(actual[i] - start, direction) / lengthSquared)
                        : 0f;
                    Vector3 projected = start + direction * t;
                    float distance = Vector3.Distance(actual[i], projected);
                    if (distance >= nearestDistance) continue;
                    nearestDistance = distance;
                    nearestReferenceLength = cumulativeLengths[segment] + segmentLengths[segment] * t;
                }

                totalDeviation += nearestDistance;
                if (i > 0) totalTrackingError += Mathf.Abs(actualLength - nearestReferenceLength);
            }

            return new RehabTrajectoryMetrics
            {
                TrajectoryDeviationMm = totalDeviation / actual.Count * 1000f,
                TrajectoryTrackingErrorMm = totalTrackingError * 1000f,
                HasData = true
            };
        }
    }
}
