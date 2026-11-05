using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class GroupSpeakerPresenter : SubtitlePresenter
    {
        [SerializeField, Tooltip("empty for all speakers")] private List<SpeakerProfile> speakers = new();
        [SerializeField] private TMP_Text text;

        private SubtitleLineBlock block;
        private float authoredFontSize;

        public override bool Covers(SpeakerProfile speaker) => speakers.Count == 0 || speakers.Contains(speaker);

        protected override bool HasContent => block.HasLines;
        protected override float AuthoredFontSize => authoredFontSize;

        protected override void Awake()
        {
            base.Awake();
            authoredFontSize = text.fontSize;
            block = new SubtitleLineBlock(text);
        }

        protected override void Sync(IReadOnlyList<SegmentInstance> visible)
        {
            TextStylePreferences style = TextStyle(SubtitleRuntime.Preferences.Current);
            block.SetLines(visible, Covers, style.showSpeakerName, style.colorNameBySpeaker);
        }

        protected override void ApplyStyle(SubtitlePreferences preferences)
        {
            TextStylePreferences style = TextStyle(preferences);
            SubtitleStyler.Apply(style, text, null, Background, null);
            block.Rebuild(style.showSpeakerName, style.colorNameBySpeaker);
        }

        protected override void UpdateReveal(double now)
        {
            block.UpdateReveal(SubtitleRuntime.Preferences.Current.entranceStyle, now);
        }
    }
}
