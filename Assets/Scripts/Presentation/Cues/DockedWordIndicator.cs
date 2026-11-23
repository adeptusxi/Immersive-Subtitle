using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    // shows speaker's most recent word
    public class DockedWordIndicator : DockedIndicator
    {
        [SerializeField] private TMP_Text word;
        [SerializeField] private bool textOnArrow = true;

        private static readonly Regex tags = new("<[^>]*>");
        private const float flipThreshold = 0.1f; 

        private readonly List<string> words = new();
        private SpeakerProfile speaker;
        private SegmentInstance segment;
        private int shownIndex = -1;
        private bool pointsLeft;

        public override void Bind(SpeakerProfile speaker)
        {
            base.Bind(speaker);
            this.speaker = speaker;
            word.text = string.Empty;
        }

        public override void OnPlaced(Transform head, Vector3 target)
        {
            base.OnPlaced(head, target);
            if (!textOnArrow)
            {
                return;
            }

            if (Mathf.Abs(ArrowDirection.x) > flipThreshold)
            {
                pointsLeft = ArrowDirection.x < 0f;
            }

            // rotate text to be mostly right-side-up on the arrow which points in pivot's local +y
            word.rectTransform.localRotation = Quaternion.Euler(0f, 0f, pointsLeft ? -90f : 90f);
        }

        public void ApplyStyle(TextStylePreferences style)
        {
            word.font = style.font != null ? style.font : TMP_Settings.defaultFontAsset;
            word.color = speaker.color;
        }

        public void SetSegment(SegmentInstance segment)
        {
            if (this.segment == segment)
            {
                return;
            }

            this.segment = segment;
            shownIndex = -1;
            words.Clear();
            if (segment == null)
            {
                word.text = string.Empty;
                return;
            }

            foreach (string token in tags.Replace(segment.text, string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (HasLetterOrDigit(token))
                {
                    words.Add(token);
                }
            }
        }

        public void UpdateWord(double now)
        {
            if (segment == null || words.Count == 0 || now < segment.startedAt)
            {
                return;
            }

            double progress = segment.plannedDuration > 0 ? (now - segment.startedAt) / segment.plannedDuration : 1;
            int index = Math.Min(words.Count - 1, (int)Math.Floor(progress * words.Count));
            if (index != shownIndex)
            {
                shownIndex = index;
                word.text = words[index];
            }
        }

        private static bool HasLetterOrDigit(string token)
        {
            foreach (char c in token)
            {
                if (char.IsLetterOrDigit(c))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
