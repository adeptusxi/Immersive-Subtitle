using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class DockedHUDCue : DockedCue<DockedSubtitlePanel>
    {
        protected override SpeechCueMode Mode => SpeechCueMode.DockedHUD;

        protected override bool IsSpeaking(SpeakerQueue queue)
        {
            foreach (SegmentInstance segment in SubtitleRuntime.ActiveSubtitles.Visible)
            {
                if (segment.speaker == queue.Profile)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ApplyPreferences(SubtitlePreferences preferences)
        {
            foreach (DockedSubtitlePanel panel in Items.Values)
            {
                panel.ApplyStyle(preferences.text);
            }
        }

        protected override void OnShown(DockedSubtitlePanel panel, SpeakerProfile speaker)
        {
            panel.ApplyStyle(SubtitleRuntime.Preferences.Current.text);
        }

        protected override void UpdateItem(DockedSubtitlePanel panel, SpeakerProfile speaker)
        {
            SubtitlePreferences preferences = SubtitleRuntime.Preferences.Current;
            panel.SetLines(SubtitleRuntime.ActiveSubtitles.Visible);
            panel.UpdatePanel(preferences, SubtitleRuntime.ActiveSubtitles.Now, Player.Instance.Head);
        }
    }
}

