using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class DockedIndicatorCue : DockedCue<DockedIndicator>
    {
        protected override SpeechCueMode Mode => SpeechCueMode.DockedIndicator;

        protected override bool IsSpeaking(SpeakerQueue queue) => !queue.IsIdle;
    }
}
