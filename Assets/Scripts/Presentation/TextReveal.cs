using System;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;

namespace ImmersiveSubtitles.Presentation
{
    public static class TextReveal
    {
        private const double RevealFraction = 0.6; // percentage of planned duration when the text should be fully visible 

        // wordEndIndices: # visible characters at the end of each word (last character index + 1). 
        // elapsed will be clamped if negative 
        public static int VisibleCharacters(EntranceStyle style, int totalCharacters, IReadOnlyList<int> wordEndIndices, double elapsed, double plannedDuration)
        {
            if (style == EntranceStyle.Instant || plannedDuration <= 0)
            {
                return totalCharacters;
            }

            double progress = Math.Min(1.0, Math.Max(0.0, elapsed) / (RevealFraction * plannedDuration));

            if (style == EntranceStyle.ByCharacter)
            {
                return (int)Math.Ceiling(progress * totalCharacters);
            }

            int words = (int)Math.Ceiling(progress * wordEndIndices.Count);
            return words == 0 ? 0 : wordEndIndices[words - 1];
        }
    }
}
