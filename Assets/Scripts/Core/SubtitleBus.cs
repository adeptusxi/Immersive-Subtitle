using System;

namespace ImmersiveSubtitles.Core
{
    public static class SubtitleBus
    {
        public static event Action<SegmentPreroll> SegmentPrerolling;
        public static event Action<SegmentInstance> SegmentStarted;
        public static event Action<SegmentInstance, SegmentEndReason> SegmentEnded;
        public static event Action<ChoiceRecord> ChoiceMade;
        public static event Action NarrativeEnded;
        public static event Action NarrativeRestarted; 

        public static void RaiseSegmentPrerolling(SegmentPreroll preroll) => SegmentPrerolling?.Invoke(preroll);

        public static void RaiseSegmentStarted(SegmentInstance segment) => SegmentStarted?.Invoke(segment);

        public static void RaiseSegmentEnded(SegmentInstance segment, SegmentEndReason reason) => SegmentEnded?.Invoke(segment, reason);

        public static void RaiseChoiceMade(ChoiceRecord choice) => ChoiceMade?.Invoke(choice);

        public static void RaiseNarrativeEnded() => NarrativeEnded?.Invoke();

        public static void RaiseNarrativeRestarted() => NarrativeRestarted?.Invoke();
    }
}
