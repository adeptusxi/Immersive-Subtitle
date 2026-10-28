using System;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    public sealed class SegmentInstance
    {
        public Guid id;
        public string turnId;
        public int indexInTurn;
        public string text; // can be rich text (<i><b><u>)
        public SpeakerProfile speaker;
        public Transform anchor; 
        public double plannedDuration;
        public double plannedGap;
        public double startedAt; // in narrative time
    }
}
