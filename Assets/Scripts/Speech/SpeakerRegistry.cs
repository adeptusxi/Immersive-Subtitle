using System;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    public sealed class SpeakerRegistry
    {
        public static SpeakerRegistry Global { get; } = new();

        private readonly List<SpeakerQueue> queues = new();
        private readonly Dictionary<SpeakerProfile, Transform> roots = new();

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

        public void Register(SpeakerQueue queue, Transform root)
        {
            if (TryGet(queue.Profile.id, out _))
            {
                Debug.LogError($"[Subtitles] duplicate speaker id '{queue.Profile.id}', ignoring the second speaker");
                return;
            }
            queues.Add(queue);
            roots[queue.Profile] = root;
        }

        public void Unregister(SpeakerQueue queue)
        {
            if (queues.Remove(queue))
            {
                roots.Remove(queue.Profile);
            }
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
        
        // null if speaker isn't registered or has no speakerRoot
        public Transform RootOf(SpeakerProfile speaker)
        {
            return roots.GetValueOrDefault(speaker);
        }

        public void StopAll()
        {
            // snapshot: stop handlers may unregister speakers
            foreach (SpeakerQueue queue in queues.ToArray())
                queue.Stop();
        }
    }
}
