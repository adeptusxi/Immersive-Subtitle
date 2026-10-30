using System;

namespace ImmersiveSubtitles.Core
{
    public sealed class SegmentInstance
    {
        public Guid id;
        public string turnId;
        public int indexInTurn;
        public string text; // can be rich text (<i><b><u>)
        public SpeakerProfile speaker;
        public double plannedDuration;
        public double plannedGap;
        public double startedAt; // in narrative time
        public double volume = 1; // multiplier on Voice volume
        public double textSize = 1; // multiplier on subtitle font size
    }
}