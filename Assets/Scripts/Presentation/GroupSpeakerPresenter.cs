using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class GroupSpeakerPresenter : SubtitlePresenter
    {
        [SerializeField, Tooltip("empty for all speakers")] private List<SpeakerProfile> speakers = new();
        [SerializeField] private TMP_Text text;

        private readonly List<SegmentInstance> lines = new();
        private readonly List<int> wordEnds = new();
        private readonly StringBuilder builder = new();
        private float authoredFontSize;
        private int bodyStartIdx; // string index where the newest line's body starts
        private int numPrefixCharacters; // # visible characters before the newest line's body
        private bool revealDirty;

        public override bool Covers(SpeakerProfile speaker) => speakers.Count == 0 || speakers.Contains(speaker);

        protected override bool HasContent => lines.Count > 0;
        protected override float AuthoredFontSize => authoredFontSize;

        protected override void Awake()
        {
            base.Awake();
            authoredFontSize = text.fontSize;
        }

        protected override void Sync(IReadOnlyList<SegmentInstance> visible)
        {
            int count = 0;
            bool same = true;
            foreach (SegmentInstance segment in visible)
            {
                if (!Covers(segment.speaker))
                {
                    continue;
                }

                same &= count < lines.Count && lines[count] == segment;
                count++;
            }

            if (same && count == lines.Count)
            {
                return;
            }

            lines.Clear();
            foreach (SegmentInstance segment in visible)
            {
                if (Covers(segment.speaker))
                {
                    lines.Add(segment);
                }
            }

            RebuildText();
        }

        protected override void ApplyStyle(SubtitlePreferences preferences)
        {
            SubtitleStyler.Apply(TextStyle(preferences), text, null, Background, null);
            RebuildText(); 
        }

        protected override void UpdateReveal(double now)
        {
            if (revealDirty)
            {
                CacheNewestLine();
                revealDirty = false;
            }

            SegmentInstance newest = lines[^1];
            int numBodyCharacters = text.textInfo.characterCount - numPrefixCharacters;
            text.maxVisibleCharacters = numPrefixCharacters + TextReveal.VisibleCharacters(SubtitleRuntime.Preferences.Current.entranceStyle, numBodyCharacters, wordEnds, now - newest.startedAt, newest.plannedDuration);
        }
        
        private void RebuildText()
        {
            TextStylePreferences style = TextStyle(SubtitleRuntime.Preferences.Current);
            builder.Clear();
            bodyStartIdx = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                SegmentInstance segment = lines[i];
                if (i > 0)
                {
                    builder.Append('\n');
                }

                if (style.showSpeakerName)
                {
                    if (style.colorNameBySpeaker)
                    {
                        builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(segment.speaker.color)).Append('>')
                            .Append(segment.speaker.displayName).Append(":</color> ");
                    }
                    else
                    {
                        builder.Append(segment.speaker.displayName).Append(": ");
                    }
                }

                bodyStartIdx = builder.Length;
                if (Math.Abs(segment.textSize - 1) > Mathf.Epsilon)
                {
                    builder.Append("<size=").Append((segment.textSize * 100).ToString(CultureInfo.InvariantCulture)).Append("%>")
                        .Append(segment.text).Append("</size>");
                }
                else
                {
                    builder.Append(segment.text);
                }
            }

            text.text = builder.ToString();
            revealDirty = true;
        }
        
        private void CacheNewestLine()
        {
            text.maxVisibleCharacters = int.MaxValue;
            text.ForceMeshUpdate();

            TMP_TextInfo info = text.textInfo;
            numPrefixCharacters = 0;
            while (numPrefixCharacters < info.characterCount && info.characterInfo[numPrefixCharacters].index < bodyStartIdx)
            {
                numPrefixCharacters++;
            }

            wordEnds.Clear();
            for (int i = numPrefixCharacters; i < info.characterCount; i++)
            {
                bool isWordChar = !char.IsWhiteSpace(info.characterInfo[i].character);
                bool nextIsBreak = i + 1 == info.characterCount || char.IsWhiteSpace(info.characterInfo[i + 1].character);
                if (isWordChar && nextIsBreak)
                {
                    wordEnds.Add(i + 1 - numPrefixCharacters);
                }
            }
        }
    }
}
