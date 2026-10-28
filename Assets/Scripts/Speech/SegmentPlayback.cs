using System;
using ImmersiveSubtitles.Core;

namespace ImmersiveSubtitles.Speech
{
    public sealed class SegmentPlayback
    {
        public event Action<SegmentPlayback> Completed;
        public bool IsDone { get; private set; }
        public SegmentInstance Instance { get; internal set; } // null until started 

        internal void Complete()
        {
            if (IsDone)
                return;
            IsDone = true;
            Completed?.Invoke(this);
        }
    }
}
