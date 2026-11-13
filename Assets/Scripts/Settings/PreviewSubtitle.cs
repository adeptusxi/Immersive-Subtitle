using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Settings
{
    public sealed class PreviewSubtitle : MonoBehaviour
    {
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image background;
        [SerializeField] private SpeakerProfile previewSpeaker;
        [SerializeField, TextArea] private string sampleText = "This is how subtitles will look.";
        [SerializeField, Min(0.1f)] private float loopSeconds = 3f;

        private readonly List<int> wordEnds = new();
        private float authoredFontSize;
        private bool revealDirty;

        private void Awake()
        {
            authoredFontSize = body.fontSize;
            body.text = sampleText;
            nameText.text = previewSpeaker.displayName;
        }

        private void OnEnable()
        {
            SubtitleRuntime.Preferences.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            SubtitleRuntime.Preferences.Changed -= Apply;
        }

        // unscaled time since narrative clock is paused while menu is open 
        private void Update()
        {
            if (revealDirty)
            {
                CacheWordEnds();
                revealDirty = false;
            }

            double elapsed = Time.unscaledTimeAsDouble % loopSeconds;
            body.maxVisibleCharacters = TextReveal.VisibleCharacters(
                SubtitleRuntime.Preferences.Current.entranceStyle, body.textInfo.characterCount, wordEnds, elapsed, loopSeconds);
        }

        private void Apply()
        {
            TextStylePreferences style = SubtitleRuntime.Preferences.Current.text;
            SubtitleStyler.Apply(style, body, nameText, background, previewSpeaker);
            SubtitleStyler.SetEmHeight(transform, MathUtils.AngularToWorldSize(style.sizeDegrees, 1f), authoredFontSize);
            revealDirty = true;
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
