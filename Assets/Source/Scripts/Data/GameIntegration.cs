using System;
using System.Collections.Generic;

namespace LB
{
    [Serializable]
    public sealed class GameDefinition
    {
        public string Id;
        public string DisplayName;
        public string Direction;
        public TrainingMode[] Modes;
        public string IconKey;
    }

    [Serializable]
    public sealed class GameSessionConfig
    {
        public string GameId;
        public int TreatmentPlanId;
        public string PatientId;
        public TrainingMode Mode;
        public TrainingMode[] Modes;
        public TreatmentPlanItem[] GameItems;
        public string[] Directions;
        public bool IsDirectionRestricted;
        public int DurationMinutes;
        public int ResistanceLevel;
    }

    public interface IGameCatalog
    {
        IReadOnlyList<GameDefinition> Games { get; }
    }

    public interface IGameRuntime
    {
        bool IsAvailable { get; }
        bool TryStart(GameSessionConfig config, out string message);
        void Stop();
    }

    public static class GameServices
    {
        public static IGameCatalog Catalog { get; set; }
        public static IGameRuntime Runtime { get; set; }

        public static IReadOnlyList<GameDefinition> Games
        {
            get { return Catalog != null ? Catalog.Games : Array.Empty<GameDefinition>(); }
        }
    }
}
