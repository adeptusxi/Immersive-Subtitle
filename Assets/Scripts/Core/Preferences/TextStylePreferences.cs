using System;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [Serializable]
    public sealed class TextStylePreferences
    {
        // text style 
        public TMP_FontAsset font; // null -> tmp default font
        public float sizeDegrees = 1.2f; // angular em height
        public Color textColor = Color.white;
        public bool colorTextBySpeaker = false;
        
        // text presentation 
        public Color backgroundColor = Color.black;
        public float backgroundOpacity = 0.6f;

        public TextStylePreferences Clone() => (TextStylePreferences)MemberwiseClone();
    }
}
