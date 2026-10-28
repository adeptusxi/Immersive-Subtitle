using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    [DefaultExecutionOrder(-100)]
    public sealed class NarrativeClock : MonoBehaviour
    {
        private readonly HashSet<object> pauseOwners = new();

        public double Now { get; private set; }
        public bool IsPaused => pauseOwners.Count > 0;
        public event Action<bool> PausedChanged;

        private bool verbose = true;

        public void Pause(object owner)
        {
            if (pauseOwners.Add(owner) && pauseOwners.Count == 1)
            {
                if (verbose) Debug.Log($"[Subtitles] t={Now.ToString("F2", CultureInfo.InvariantCulture)} paused");
                PausedChanged?.Invoke(true);
            }
        }

        public void Resume(object owner)
        {
            if (pauseOwners.Remove(owner) && pauseOwners.Count == 0)
            {
                if (verbose) Debug.Log($"[Subtitles] t={Now.ToString("F2", CultureInfo.InvariantCulture)} resumed");
                PausedChanged?.Invoke(false);
            }
        }

        private void Update()
        {
            if (!IsPaused)
                Now += Time.unscaledDeltaTime;
        }
    }
}
