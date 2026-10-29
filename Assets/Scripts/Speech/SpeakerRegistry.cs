using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    public sealed class SpeakerRegistry
    {
        public static SpeakerRegistry Global { get; } = new();

        private readonly List<SpeakerQueue> queues = new();

        public IReadOnlyList<SpeakerQueue> All => queues;

        public bool AllIdle
        {
            get
            {
                foreach (SpeakerQueue queue in queues)
                {
                    if (!queue.IsIdle)
                        return false;
                }
                return true;
            }
        }

        public void Register(SpeakerQueue queue)
        {
            if (TryGet(queue.Profile.id, out _))
            {
                Debug.LogError($"[Subtitles] duplicate speaker id '{queue.Profile.id}', ignoring the second speaker");
                return;
            }
            queues.Add(queue);
        }

        public void Unregister(SpeakerQueue queue)
        {
            queues.Remove(queue);
        }

        public bool TryGet(string speakerId, out SpeakerQueue queue)
        {
            foreach (SpeakerQueue candidate in queues)
            {
                if (string.Equals(candidate.Profile.id, speakerId, StringComparison.OrdinalIgnoreCase))
                {
                    queue = candidate;
                    return true;
                }
            }
            queue = null;
            return false;
        }

        public void StopAll()
        {
            // snapshot: stop handlers may unregister speakers
            foreach (SpeakerQueue queue in queues.ToArray())
                queue.Stop();
        }
    }
}
