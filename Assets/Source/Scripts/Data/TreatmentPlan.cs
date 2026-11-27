using System;
using System.Collections.Generic;
using SimpleSQL;
using UnityEngine;

namespace LB
{
    [Serializable]
    public class TreatmentPlan
    {
        [PrimaryKey, AutoIncrement]
        public int PlanID { get; set; }

        [MaxLength(64), NotNull]
        public string Name { get; set; }

        // 数据库非空列由首个游戏条目同步，业务配置以 GameItemsJson 为准。
        public int Mode { get; set; }
        public string Direction { get; set; }
        public bool IsDirectionRestricted { get; set; }
        public int DurationMinutes { get; set; }
        public int ResistanceLevel { get; set; }
        public string GameItemsJson { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        public List<TrainingMode> GetModes()
        {
            List<TrainingMode> modes = new List<TrainingMode>();
            foreach (TreatmentPlanItem item in GetGameItems())
            {
                TrainingMode mode = (TrainingMode)item.Mode;
                if (mode >= TrainingMode.Cpm && mode <= TrainingMode.Resist && !modes.Contains(mode))
                    modes.Add(mode);
            }
            return modes;
        }

        public List<TreatmentPlanItem> GetGameItems()
        {
            if (string.IsNullOrEmpty(GameItemsJson)) return new List<TreatmentPlanItem>();
            TreatmentPlanGameList saved = JsonUtility.FromJson<TreatmentPlanGameList>(GameItemsJson);
            return saved != null && saved.Items != null ? saved.Items : new List<TreatmentPlanItem>();
        }

        public void SetGameItems(List<TreatmentPlanItem> items)
        {
            GameItemsJson = JsonUtility.ToJson(new TreatmentPlanGameList { Items = items ?? new List<TreatmentPlanItem>() });
            TreatmentPlanItem first = items != null && items.Count > 0 ? items[0] : null;
            Mode = first != null ? first.Mode : (int)TrainingMode.Assist;
            Direction = first != null ? first.Direction : "";
            IsDirectionRestricted = first != null && first.IsDirectionRestricted;
            DurationMinutes = first != null ? first.DurationMinutes : 0;
            ResistanceLevel = first != null ? first.ResistanceLevel : 3;
        }
    }

    [Serializable]
    public class TreatmentPlanGameList
    {
        public List<TreatmentPlanItem> Items;
    }

    [Serializable]
    public class TreatmentPlanItem
    {
        public string GameId;
        public string DisplayName;
        public int Mode;
        public string Direction;
        public bool IsDirectionRestricted;
        public int DurationMinutes;
        public int ResistanceLevel;
    }
}
