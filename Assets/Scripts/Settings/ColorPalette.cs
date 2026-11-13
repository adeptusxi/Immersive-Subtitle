using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImmersiveSubtitles.Settings
{
    [Serializable]
    public sealed class NamedColor
    {
        public string name;
        public Color color = Color.white;
    }
    
    [CreateAssetMenu(menuName = "Immersive Subtitles/Color Palette")]
    public sealed class ColorPalette : ScriptableObject
    {
        public List<NamedColor> colors = new();
    }
}
