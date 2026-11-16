namespace ImmersiveSubtitles.Core
{
    public enum PresentationMode
    {
        SpeakerAboveHead,
        SpeakerBesideHead,
        SpeakerHalo,
        Group
    }

    public enum SubtitleSizeMode
    {
        Angular,
        Fixed
    }

    public enum EntranceStyle
    {
        Instant,
        ByWord,
        ByCharacter
    }

    public enum SpeechCueMode
    {
        None,
        RingPulse,
        SubtitleHUD,
        DockedIndicator,
        DockedWord,
        DockedHUD
    }
}
