using System;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    // keep in sync with TextStylePreferences
    [Serializable]
    public sealed class TextStyleOverride
    {
        public bool overrideFont;
        public TMP_FontAsset font;
        public bool overrideSizeDegrees;
        public float sizeDegrees = 1.2f;
        public bool overrideTextColor;
        public Color textColor = Color.white;
        public bool overrideColorTextBySpeaker;
        public bool colorTextBySpeaker = false;
        public bool overrideBackgroundColor;
        public Color backgroundColor = Color.black;
        public bool overrideBackgroundOpacity;
        public float backgroundOpacity = 0.6f;

        public void Resolve(TextStylePreferences source, TextStylePreferences result)
        {
            result.font = overrideFont ? font : source.font;
            result.sizeDegrees = overrideSizeDegrees ? sizeDegrees : source.sizeDegrees;
            result.textColor = overrideTextColor ? textColor : source.textColor;
            result.colorTextBySpeaker = overrideColorTextBySpeaker ? colorTextBySpeaker : source.colorTextBySpeaker;
            result.backgroundColor = overrideBackgroundColor ? backgroundColor : source.backgroundColor;
            result.backgroundOpacity = overrideBackgroundOpacity ? backgroundOpacity : source.backgroundOpacity;
        }
    }
}
