using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    // displays a specific speaker's current segment 
    public sealed class SingleSpeakerPresenter : SubtitlePresenter
    {
        [SerializeField] private SpeakerProfile speaker;
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private bool overrideTextStyle;
        [SerializeField] private TextStylePreferences textStyle = new();

        private readonly List<int> wordEnds = new();
        private SegmentInstance current;
        private float authoredFontSize;
        private bool revealDirty;

        public override bool Covers(SpeakerProfile other) => other == speaker;

        protected override bool HasContent => current != null;
        protected override float AuthoredFontSize => authoredFontSize;

        protected override void Awake()
        {
            base.Awake();
            authoredFontSize = body.fontSize;
        }

        protected override TextStylePreferences TextStyle(SubtitlePreferences preferences) =>
            overrideTextStyle ? textStyle : preferences.text;

        protected override void Sync(IReadOnlyList<SegmentInstance> visible)
        {
            SegmentInstance segment = null;
            for (int i = visible.Count - 1; i >= 0; i--)
            {
                if (visible[i].speaker == speaker)
                {
                    segment = visible[i];
                    break;
                }
            }

            if (segment == current)
            {
                return;
            }

            current = segment;
            if (current != null)
            {
                body.text = current.text;
                body.fontSize = authoredFontSize * (float)current.textSize;
                nameText.text = speaker.displayName;
                revealDirty = true;
            }
        }

        protected override void ApplyStyle(SubtitlePreferences preferences)
        {
            SubtitleStyler.Apply(TextStyle(preferences), body, nameText, Background, speaker);
            revealDirty = true;
        }

        protected override void UpdateReveal(double now)
        {
            if (revealDirty)
            {
                CacheWordEnds();
                revealDirty = false;
            }

            body.maxVisibleCharacters = TextReveal.VisibleCharacters(
                SubtitleRuntime.Preferences.Current.entranceStyle, body.textInfo.characterCount, wordEnds, now - current.startedAt, current.plannedDuration);
        }
        
        private void CacheWordEnds()
        {
            body.maxVisibleCharacters = int.MaxValue;
            body.ForceMeshUpdate();

            TMP_TextInfo info = body.textInfo;
            wordEnds.Clear();
            for (int i = 0; i < info.characterCount; i++)
            {
                bool isWordChar = !char.IsWhiteSpace(info.characterInfo[i].character);
                bool nextIsBreak = i + 1 == info.characterCount || char.IsWhiteSpace(info.characterInfo[i + 1].character);
                if (isWordChar && nextIsBreak)
                {
                    wordEnds.Add(i + 1);
                }
            }
        }
    }
}
