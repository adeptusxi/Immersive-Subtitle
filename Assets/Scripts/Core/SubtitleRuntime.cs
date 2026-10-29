namespace ImmersiveSubtitles.Core
{
    public static class SubtitleRuntime
    {
        public static PreferencesService Preferences { get; } = new();
        public static ActiveSubtitleRegistry ActiveSubtitles { get; } = new();

        static SubtitleRuntime()
        {
            SubtitleBus.SegmentStarted += ActiveSubtitles.HandleStarted;
            SubtitleBus.SegmentEnded += ActiveSubtitles.HandleEnded;
            Preferences.Changed += ActiveSubtitles.HandlePreferencesChanged;
        }
    }
}
