using System;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;

namespace ImmersiveSubtitles.Transcript
{
    // records the transcript 
    public sealed class TranscriptLog
    {
        private readonly List<TranscriptEntry> entries = new();
        private readonly Dictionary<string, TranscriptEntry> turns = new();

        public IReadOnlyList<TranscriptEntry> Entries => entries;
        public event Action<TranscriptEntry> EntryAdded;
        public event Action<TranscriptEntry> EntryUpdated;

        public void InitSubscriptions()
        {
            SubtitleBus.SegmentStarted += HandleSegmentStarted;
            SubtitleBus.SegmentEnded += HandleSegmentEnded;
            SubtitleBus.ChoiceMade += HandleChoiceMade;
        }

        public void CleanupSubscriptions()
        {
            SubtitleBus.SegmentStarted -= HandleSegmentStarted;
            SubtitleBus.SegmentEnded -= HandleSegmentEnded;
            SubtitleBus.ChoiceMade -= HandleChoiceMade;
        }

        public void Clear()
        {
            entries.Clear();
            turns.Clear();
        }

        private void HandleSegmentStarted(SegmentInstance segment)
        {
            if (turns.TryGetValue(segment.turnId, out TranscriptEntry entry))
            {
                entry.segments.Add(segment);
                EntryUpdated?.Invoke(entry);
                return;
            }

            entry = new TranscriptEntry
            {
                type = TranscriptEntryType.Turn,
                speaker = segment.speaker,
                turnId = segment.turnId,
            };
            
            entry.segments.Add(segment);
            turns.Add(segment.turnId, entry);
            entries.Add(entry);
            EntryAdded?.Invoke(entry);
        }

        private void HandleSegmentEnded(SegmentInstance segment, SegmentEndReason reason)
        {
            if (reason != SegmentEndReason.Interrupted || !turns.TryGetValue(segment.turnId, out TranscriptEntry entry))
            {
                return;
            }

            entry.interrupted = true;
            EntryUpdated?.Invoke(entry);
        }

        private void HandleChoiceMade(ChoiceRecord choice)
        {
            var entry = new TranscriptEntry { type = TranscriptEntryType.Choice, text = choice.label };
            entries.Add(entry);
            EntryAdded?.Invoke(entry);
        }
    }
}
