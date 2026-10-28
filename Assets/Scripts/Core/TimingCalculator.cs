using System;

namespace ImmersiveSubtitles.Core
{
    public static class TimingCalculator
    {
        public static SegmentTiming Compute(string countedText, double rate, TimingProfile profile)
        {
            double raw = profile.baseSeconds
                         + CountWords(countedText) * profile.secondsPerWord
                         + CountChars(countedText) * profile.secondsPerChar;
            double clamped = Math.Min(Math.Max(raw, profile.minSegmentSeconds), profile.maxSegmentSeconds);

            string trimmed = countedText.TrimEnd();
            double gap = trimmed.Length > 0 ? profile.GapFor(trimmed[trimmed.Length - 1]) : profile.defaultGap;

            return new SegmentTiming
            {
                duration = clamped / rate,
                gap = gap / rate,
                hitMax = raw > profile.maxSegmentSeconds,
            };
        }

        // counts whitespace delimited tokens containing at least one letter/digit 
        public static int CountWords(string countedText)
        {
            int words = 0;
            bool inToken = false;
            bool tokenHasWordChar = false;
            foreach (char c in countedText)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (inToken && tokenHasWordChar)
                        words++;
                    inToken = false;
                    tokenHasWordChar = false;
                }
                else
                {
                    inToken = true;
                    tokenHasWordChar |= char.IsLetterOrDigit(c);
                }
            }
            if (inToken && tokenHasWordChar)
                words++;
            return words;
        }

        // counts non whitespace characters
        public static int CountChars(string countedText)
        {
            int chars = 0;
            foreach (char c in countedText)
            {
                if (!char.IsWhiteSpace(c))
                    chars++;
            }
            return chars;
        }
    }
}
