using System;

namespace ImmersiveSubtitles.Core
{
    [Serializable]
    public sealed class SubtitlePreferences
    {
        public PresentationMode mode = PresentationMode.SpeakerAboveHead;
        public EntranceStyle entranceStyle = EntranceStyle.Instant;
        public bool keepTextDuringGaps = true;
        public bool facePlayer = true;
        public bool lagFollow = true;
        public SubtitleSizeMode sizeMode = SubtitleSizeMode.Angular;
        public SpeechCueMode speechCueMode = SpeechCueMode.DockedIndicator;

        public TextStylePreferences text = new();

        public SubtitlePreferences Clone()
        {
            SubtitlePreferences copy = (SubtitlePreferences)MemberwiseClone();
            copy.text = text.Clone();
            return copy;
        }
    }
}
