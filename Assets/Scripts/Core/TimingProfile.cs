using System.Collections.Generic;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [CreateAssetMenu(menuName = "Immersive Subtitles/Timing Profile")]
    public sealed class TimingProfile : ScriptableObject
    {
        public double baseSeconds = 0.3;
        public double secondsPerWord = 0.25;
        public double secondsPerChar = 0.02;
        public double minSegmentSeconds = 0.8;
        public double maxSegmentSeconds = 6.0;
        public double defaultGap = 0.1;
        
        public List<PunctuationGap> punctuationGaps = new List<PunctuationGap>
        {
            new PunctuationGap { characters = ",;:", seconds = 0.15 },
            new PunctuationGap { characters = ".!?", seconds = 0.35 },
            new PunctuationGap { characters = "—…-", seconds = 0.3 },
        };
        
        public double GapFor(char lastNonSpace)
        {
            foreach (PunctuationGap entry in punctuationGaps)
            {
                if (entry.characters != null && entry.characters.IndexOf(lastNonSpace) >= 0)
                    return entry.seconds;
            }
            return defaultGap; //note: not divided by rate
        }
    }
}
