using System;

namespace ImmersiveSubtitles.Core
{
    [Flags]
    public enum PresenterMode
    {
        None = 0,
        SpeakerAttached = 1,
        Collector = 2
    }

    public enum EntranceStyle
    {
        Instant,
        ByWord,
        ByCharacter
    }

    public enum SubtitleAlignment
    {
        Left,
        Center,
        Right
    }
}
