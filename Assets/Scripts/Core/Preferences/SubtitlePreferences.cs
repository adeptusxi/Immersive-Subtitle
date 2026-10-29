using System;

namespace ImmersiveSubtitles.Core
{
    [Serializable]
    public sealed class SubtitlePreferences
    {
        public PresenterMode presenters = PresenterMode.SpeakerAttached;
        public EntranceStyle entranceStyle = EntranceStyle.Instant;
        public bool keepTextDuringGaps = false;

        public TextStylePreferences text = new();

        public SubtitlePreferences Clone()
        {
            SubtitlePreferences copy = (SubtitlePreferences)MemberwiseClone();
            copy.text = text.Clone();
            return copy;
        }
    }
}
