using System;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class HUDCue : VisualSpeechCue
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image background;

        private HashSet<SpeakerProfile> outOfView = new();
        private HashSet<SpeakerProfile> wasOutOfView = new();
        private SubtitleLineBlock block;
        private Predicate<SpeakerProfile> isOutOfView;
        private float authoredFontSize;

        protected override SpeechCueMode Mode => SpeechCueMode.SubtitleHUD;

        private void Awake()
        {
            authoredFontSize = text.fontSize;
            block = new SubtitleLineBlock(text);
            isOutOfView = speaker => outOfView.Contains(speaker);
            root.SetActive(false);
        }

        protected override void ApplyPreferences(SubtitlePreferences preferences)
        {
            SubtitleStyler.Apply(preferences.text, text, null, background, null);
            block.Rebuild(true, preferences.text.colorNameBySpeaker);
        }

        protected override void UpdateCue()
        {
            var preferences = SubtitleRuntime.Preferences.Current;
            var visible = SubtitleRuntime.ActiveSubtitles.Visible;

            (outOfView, wasOutOfView) = (wasOutOfView, outOfView);
            outOfView.Clear();
            foreach (var segment in visible)
            {
                Transform speakerRoot = SpeakerRegistry.Global.RootOf(segment.speaker);
                if (speakerRoot != null && IsOutOfView(speakerRoot, wasOutOfView.Contains(segment.speaker)))
                {
                    outOfView.Add(segment.speaker);
                }
            }

            block.SetLines(visible, isOutOfView, true, preferences.text.colorNameBySpeaker);

            if (block.HasLines != root.activeSelf)
            {
                root.SetActive(block.HasLines);
            }

            if (block.HasLines)
            {
                block.UpdateReveal(preferences.entranceStyle, SubtitleRuntime.ActiveSubtitles.Now);

                Transform panel = root.transform;
                float distance = Vector3.Distance(panel.position, Player.Instance.Head.position);
                SubtitleStyler.SetEmHeight(panel, MathUtils.AngularToWorldSize(preferences.text.sizeDegrees, distance), authoredFontSize);
            }
        }

        protected override void HideCue()
        {
            root.SetActive(false);
            outOfView.Clear();
        }
    }
}
