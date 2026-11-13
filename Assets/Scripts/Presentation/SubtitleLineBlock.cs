using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EasyTextEffects;
using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    // tmp text showing multiple speakers' segments 
    public sealed class SubtitleLineBlock
    {
        private readonly TMP_Text text;
        private readonly TextEffect effect; // optional
        private readonly List<SegmentInstance> lines = new();
        private readonly List<int> wordEnds = new();
        private readonly StringBuilder builder = new();
        private int bodyStartIdx; // string index where the newest line's body starts
        private int numPrefixCharacters; // # visible characters before the newest line's body
        private bool revealDirty;

        public bool HasLines => lines.Count > 0;

        public SubtitleLineBlock(TMP_Text text, TextEffect effect)
        {
            this.text = text;
            this.effect = effect;
            if (effect != null)
            {
                effect.enabled = false;
                effect.updatesPerSecond = 120;
            }
        }

        public void SetLines(IReadOnlyList<SegmentInstance> visible, Predicate<SpeakerProfile> covers, bool colorTextBySpeaker)
        {
            int count = 0;
            bool same = true;
            foreach (SegmentInstance segment in visible)
            {
                if (!covers(segment.speaker))
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
                if (covers(segment.speaker))
                {
                    lines.Add(segment);
                }
            }

            Rebuild(colorTextBySpeaker);
        }

        public void Rebuild(bool colorTextBySpeaker)
        {
            builder.Clear();
            bodyStartIdx = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                SegmentInstance segment = lines[i];
                if (i > 0)
                {
                    builder.Append('\n');
                }

                string speakerColor = ColorUtility.ToHtmlStringRGBA(segment.speaker.color);
                builder.Append("<color=#").Append(speakerColor).Append('>').Append(segment.speaker.displayName).Append(":</color> ");

                bodyStartIdx = builder.Length;
                if (colorTextBySpeaker)
                {
                    builder.Append("<color=#").Append(speakerColor).Append('>');
                }

                bool sized = Math.Abs(segment.textSize - 1) > Mathf.Epsilon;
                if (sized)
                {
                    builder.Append("<size=").Append((segment.textSize * 100).ToString(CultureInfo.InvariantCulture)).Append("%>");
                }

                builder.Append(segment.text);

                if (sized)
                {
                    builder.Append("</size>");
                }

                if (colorTextBySpeaker)
                {
                    builder.Append("</color>");
                }
            }

            text.text = builder.ToString();
            revealDirty = true;
        }

        public void UpdateReveal(EntranceStyle style, double now)
        {
            if (revealDirty)
            {
                CacheNewestLine();
                revealDirty = false;
            }

            SegmentInstance newest = lines[^1];
            int numBodyCharacters = text.textInfo.characterCount - numPrefixCharacters;
            text.maxVisibleCharacters = numPrefixCharacters + TextReveal.VisibleCharacters(style, numBodyCharacters, wordEnds, now - newest.startedAt, newest.plannedDuration);

            if (effect != null)
            {
                effect.Update();
            }
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

            if (effect != null)
            {
                effect.Refresh();
            }
        }
    }
}
