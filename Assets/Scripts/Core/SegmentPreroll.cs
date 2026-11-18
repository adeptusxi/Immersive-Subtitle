using System;

namespace ImmersiveSubtitles.Core
{
    // raised when a segment is about to start
    public sealed class SegmentPreroll
    {
        public SpeakerProfile speaker;
        public int indexInTurn;
        public double Seconds { get; private set; }

        public void Request(double seconds) => Seconds = Math.Max(Seconds, seconds);
    }
}
