using System;
using System.Collections.Generic;

namespace LB
{
    public enum TrainingMode
    {
        Cpm = 0,
        Assist = 1,
        Active = 2,
        Resist = 3,
        Teaching = 4,
        Intelligent = 5
    }

    public static class TrainingCatalog
    {
        public static readonly string[] DirectionIds =
        {
            "abduction", "horizontal", "flexion", "pronation", "rotation", "extension"
        };

        public static readonly int[] DurationMinutes = { 3, 5, 10, 15, 20, 25, 30 };

        public static string GetModeName(TrainingMode mode)
        {
            switch (mode)
            {
                case TrainingMode.Cpm: return "被动";
                case TrainingMode.Assist: return "助力";
                case TrainingMode.Active: return "主动";
                case TrainingMode.Resist: return "抗阻";
                case TrainingMode.Teaching: return "示教";
                case TrainingMode.Intelligent: return "智能";
                default: return "未知";
            }
        }

        public static string GetModeReportName(TrainingMode mode)
        {
            switch (mode)
            {
                case TrainingMode.Cpm: return "CPM";
                case TrainingMode.Assist: return "Assist";
                case TrainingMode.Active: return "Active";
                case TrainingMode.Resist: return "Resist";
                case TrainingMode.Teaching: return "Teaching";
                case TrainingMode.Intelligent: return "Intelligent";
                default: return mode.ToString();
            }
        }

        public static TrainingMode ParseMode(string value)
        {
            if (string.Equals(value, "CPM", StringComparison.OrdinalIgnoreCase)) return TrainingMode.Cpm;
            if (string.Equals(value, "Assist", StringComparison.OrdinalIgnoreCase)) return TrainingMode.Assist;
            if (string.Equals(value, "Active", StringComparison.OrdinalIgnoreCase)) return TrainingMode.Active;
            if (string.Equals(value, "Resist", StringComparison.OrdinalIgnoreCase)) return TrainingMode.Resist;
            if (string.Equals(value, "Teaching", StringComparison.OrdinalIgnoreCase)) return TrainingMode.Teaching;
            if (string.Equals(value, "Intelligent", StringComparison.OrdinalIgnoreCase)) return TrainingMode.Intelligent;
            return TrainingMode.Cpm;
        }

        public static string GetDirectionName(string direction)
        {
            switch (direction)
            {
                case "horizontal": return "水平面";
                case "flexion": return "矢状面";
                case "pronation": return "冠状轴";
                case "rotation": return "垂直轴";
                case "extension": return "矢状轴";
                default: return "冠状面";
            }
        }

        public static string GetDirectionNames(IEnumerable<string> directions)
        {
            List<string> names = new List<string>();
            if (directions != null)
            {
                foreach (string direction in directions)
                {
                    if (string.IsNullOrEmpty(direction)) continue;
                    names.Add(GetDirectionName(direction));
                }
            }
            return names.Count > 0 ? string.Join("、", names) : "非限制方向";
        }

        public static string GetDirectionSummaryGroup(string direction)
        {
            return GetDirectionName(direction);
        }

        public static string[] ParseDirections(string value)
        {
            if (string.IsNullOrEmpty(value)) return new string[0];
            string[] parts = value.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> directions = new List<string>(parts.Length);
            foreach (string part in parts)
            {
                string direction = part.Trim();
                if (Array.IndexOf(DirectionIds, direction) >= 0 && !directions.Contains(direction)) directions.Add(direction);
            }
            return directions.ToArray();
        }

        public static string GetModeSummaryGroup(string mode)
        {
            TrainingMode parsed = ParseMode(mode);
            if (parsed == TrainingMode.Teaching) return "被动";
            return GetModeName(parsed);
        }
    }
}
