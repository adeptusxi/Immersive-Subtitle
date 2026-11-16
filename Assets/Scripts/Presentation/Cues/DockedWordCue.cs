using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class DockedWordCue : DockedCue<DockedWordIndicator>
    {
        private readonly Dictionary<SpeakerProfile, SegmentInstance> latest = new(); // most recently started segment per speaker. kept through gaps until the speaker's queue goes idle

        protected override SpeechCueMode Mode => SpeechCueMode.DockedWord;

        protected override void OnEnable()
        {
            base.OnEnable();
            SubtitleBus.SegmentStarted += HandleSegmentStarted;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SubtitleBus.SegmentStarted -= HandleSegmentStarted;
            latest.Clear();
        }

        protected override bool IsSpeaking(SpeakerQueue queue) => !queue.IsIdle;

        protected override void UpdateCue()
        {
            base.UpdateCue();

            foreach (var queue in SpeakerRegistry.Global.All)
            {
                if (queue.IsIdle)
                {
                    latest.Remove(queue.Profile);
                }
            }
        }

        protected override void ApplyPreferences(SubtitlePreferences preferences)
        {
            foreach (DockedWordIndicator item in Items.Values)
            {
                item.ApplyStyle(preferences.text);
            }
        }

        protected override void OnShown(DockedWordIndicator item, SpeakerProfile speaker)
        {
            item.ApplyStyle(SubtitleRuntime.Preferences.Current.text);
        }

        protected override void OnHidden(DockedWordIndicator item, SpeakerProfile speaker)
        {
            item.SetSegment(null);
        }

        protected override void UpdateItem(DockedWordIndicator item, SpeakerProfile speaker)
        {
            latest.TryGetValue(speaker, out SegmentInstance segment);
            item.SetSegment(segment);
            item.UpdateWord(SubtitleRuntime.ActiveSubtitles.Now);
        }

        private void HandleSegmentStarted(SegmentInstance segment)
        {
            latest[segment.speaker] = segment;
        }
    }
}
