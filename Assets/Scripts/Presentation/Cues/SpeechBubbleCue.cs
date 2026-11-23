using ImmersiveSubtitles.Core;

namespace ImmersiveSubtitles.Presentation
{
    // docked word in a comic speech bubble 
    public sealed class SpeechBubbleCue : DockedWordCue
    {
        protected override SpeechCueMode Mode => SpeechCueMode.SpeechBubble;
    }
}
