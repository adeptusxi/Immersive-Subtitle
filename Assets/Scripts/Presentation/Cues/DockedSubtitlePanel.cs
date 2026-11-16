using System;
using System.Collections.Generic;
using EasyTextEffects;
using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    // docked text with full subtitle content per speaker 
    [RequireComponent(typeof(RectTransform))]
    public sealed class DockedSubtitlePanel : DockedItem
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image background;
        [SerializeField] private TextEffect textEffect;

        private SubtitleLineBlock block;
        private Predicate<SpeakerProfile> covers;
        private SpeakerProfile speaker;
        private float authoredFontSize;
        private Vector3 emScale;
        private bool flipped;

        private const float flipThreshold = 0.1f; // near vertical directions keep their last side so the panel doesn't flicker

        public override Vector2 HalfExtents => Size * 0.5f;

        protected override Vector3 BaseScale => emScale;

        public override void Bind(SpeakerProfile speaker)
        {
            this.speaker = speaker;
            authoredFontSize = text.fontSize;
            block = new SubtitleLineBlock(text, textEffect, showNames: false);
            covers = s => s == this.speaker;
            emScale = transform.localScale;
        }

        public void ApplyStyle(TextStylePreferences style)
        {
            SubtitleStyler.Apply(style, text, null, background, null);
            block.Rebuild(true);
        }

        public void SetLines(IReadOnlyList<SegmentInstance> visible)
        {
            block.SetLines(visible, covers, true);
        }

        public void UpdatePanel(SubtitlePreferences preferences, double now, Transform head)
        {
            block.UpdateReveal(preferences.entranceStyle, now);

            float distance = Vector3.Distance(transform.position, head.position);
            SubtitleStyler.SetEmHeight(transform, MathUtils.AngularToWorldSize(preferences.text.sizeDegrees, distance), authoredFontSize);
            emScale = transform.localScale;
        }

        // background sprite is authored pointing left. flip sprite and text if speaker is on right 
        public override void OnPlaced(Transform head, Vector3 target)
        {
            if (Mathf.Abs(Direction.x) > flipThreshold)
            {
                flipped = Direction.x > 0f;
            }

            SetMirrored(background.rectTransform, flipped);
            SetMirrored(text.rectTransform, flipped);
        }

        private static void SetMirrored(Transform target, bool mirrored)
        {
            Vector3 scale = target.localScale;
            scale.x = Mathf.Abs(scale.x) * (mirrored ? -1f : 1f);
            target.localScale = scale;
        }
    }
}
