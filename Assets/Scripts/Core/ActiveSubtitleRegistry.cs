using System;
using System.Collections.Generic;

namespace ImmersiveSubtitles.Core
{
    // decides which segments a subtitle presenter displays 
    public sealed class ActiveSubtitleRegistry
    {
        private readonly List<SegmentInstance> visible = new();
        private readonly HashSet<SegmentInstance> held = new(); // finished segments that are staying during a gap because of keepTextDuringGaps

        public IReadOnlyList<SegmentInstance> Visible => visible;
        public double Now { get; private set; } // time of last Tick
        public event Action Changed;

        private static bool KeepTextDuringGaps => SubtitleRuntime.Preferences.Current.keepTextDuringGaps;

        public void HandleStarted(SegmentInstance segment)
        {
            visible.RemoveAll(other => held.Contains(other) && other.speaker == segment.speaker);
            held.RemoveWhere(other => other.speaker == segment.speaker);

            visible.Add(segment);
            Changed?.Invoke();
        }

        public void HandleEnded(SegmentInstance segment, SegmentEndReason reason)
        {
            if (!visible.Contains(segment))
            {
                return;
            }

            if (reason == SegmentEndReason.Finished && KeepTextDuringGaps)
            {
                held.Add(segment);
                return;
            }

            held.Remove(segment);
            visible.Remove(segment);
            Changed?.Invoke();
        }

        public void Tick(double now)
        {
            Now = now;
            if (held.Count == 0)
            {
                return;
            }

            int removed = visible.RemoveAll(segment => held.Contains(segment) && now >= segment.startedAt + segment.plannedDuration + segment.plannedGap);
            if (removed == 0)
            {
                return;
            }

            held.RemoveWhere(segment => !visible.Contains(segment));
            Changed?.Invoke();
        }

        public void HandlePreferencesChanged()
        {
            if (KeepTextDuringGaps || held.Count == 0)
            {
                return;
            }

            visible.RemoveAll(held.Contains);
            held.Clear();
            Changed?.Invoke();
        }
    }
}
