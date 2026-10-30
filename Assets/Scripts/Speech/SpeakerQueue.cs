using System;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    public sealed class SpeakerQueue
    {
        private enum Phase { Delay, Showing, Gap }

        private sealed class Entry
        {
            public SegmentRequest request;
            public SegmentPlayback playback;
            public SegmentTiming timing;
            public Phase phase;
            public double startAt;
            public double EndAt => startAt + timing.duration;
            public double CompleteAt => EndAt + timing.gap;
        }

        private readonly TimingProfile timing;
        private readonly Queue<Entry> pending = new();
        private Entry current;

        // narrative time the queue has processed up to. the next request's delay starts here 
        private double cursor;
        private double lastTickNow;
        private bool hasTicked;
        private bool processing;

        public SpeakerQueue(SpeakerProfile profile, TimingProfile timing)
        {
            Profile = profile;
            this.timing = timing;
        }

        public SpeakerProfile Profile { get; }
        public bool IsIdle => current == null && pending.Count == 0;
        public event Action<SegmentInstance> SegmentStarted;
        public event Action<SegmentInstance, SegmentEndReason> SegmentEnded;

        public SegmentPlayback Enqueue(SegmentRequest request)
        {
            SegmentTiming segmentTiming = TimingCalculator.Compute(request.countedText, request.rate, timing);
            if (segmentTiming.hitMax)
                Debug.LogWarning($"[Subtitles] segment hit maxSegmentSeconds: \"{request.displayText}\"");

            var entry = new Entry { request = request, playback = new SegmentPlayback(), timing = segmentTiming };
            if (IsIdle && !processing)
                cursor = lastTickNow;
            pending.Enqueue(entry);

            if (hasTicked && !processing)
                Process(lastTickNow);
            return entry.playback;
        }

        public void Tick(double now)
        {
            processing = false;
            lastTickNow = now;
            hasTicked = true;
            if (current == null)
                cursor = now;
            Process(now);
        }

        public void Stop()
        {
            Entry stopped = current;
            current = null;
            Entry[] queued = pending.ToArray();
            pending.Clear();

            // hold off processing so that every stopped handle completes before requests enqueued by these handlers start 
            bool wasProcessing = processing;
            processing = true;
            if (stopped != null)
            {
                if (stopped.phase == Phase.Showing)
                    RaiseEnded(stopped.playback.Instance, SegmentEndReason.Interrupted);
                stopped.playback.Complete();
            }

            foreach (Entry entry in queued)
            {
                entry.playback.Complete();
            }
            processing = wasProcessing;

            if (!processing)
            {
                cursor = lastTickNow;
                if (hasTicked)
                    Process(lastTickNow);
            }
        }

        // processes every boundary <= upTo
        private void Process(double upTo)
        {
            if (processing)
            {
                return;
            }
            
            processing = true;
            
            while (Step(upTo)) { }

            if (IsIdle)
            {
                cursor = upTo;
            }
            
            processing = false;
        }

        private bool Step(double upTo)
        {
            if (current == null)
            {
                if (pending.Count == 0)
                    return false;
                current = pending.Dequeue();
                current.phase = Phase.Delay;
                current.startAt = cursor + current.request.delaySeconds;
                return true;
            }

            Entry entry = current;
            switch (entry.phase)
            {
                case Phase.Delay:
                    if (entry.startAt > upTo)
                        return false;
                    cursor = entry.startAt;
                    entry.phase = Phase.Showing;
                    entry.playback.Instance = new SegmentInstance
                    {
                        id = Guid.NewGuid(),
                        turnId = entry.request.turnId,
                        indexInTurn = entry.request.indexInTurn,
                        text = entry.request.displayText,
                        speaker = Profile,
                        plannedDuration = entry.timing.duration,
                        plannedGap = entry.timing.gap,
                        startedAt = entry.startAt,
                        volume = entry.request.volume,
                        textSize = entry.request.textSize,
                    };
                    SegmentStarted?.Invoke(entry.playback.Instance);
                    SubtitleBus.RaiseSegmentStarted(entry.playback.Instance);
                    return true;

                case Phase.Showing:
                    if (entry.EndAt > upTo)
                        return false;
                    cursor = entry.EndAt;
                    entry.phase = Phase.Gap;
                    RaiseEnded(entry.playback.Instance, SegmentEndReason.Finished);
                    return true;

                default:
                    if (entry.CompleteAt > upTo)
                        return false;
                    cursor = entry.CompleteAt;
                    current = null;
                    entry.playback.Complete();
                    return true;
            }
        }

        private void RaiseEnded(SegmentInstance instance, SegmentEndReason reason)
        {
            SegmentEnded?.Invoke(instance, reason);
            SubtitleBus.RaiseSegmentEnded(instance, reason);
        }
    }
}
