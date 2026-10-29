using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [CreateAssetMenu(menuName = "Immersive Subtitles/Font Catalog")]
    public sealed class FontCatalog : ScriptableObject
    {
        public List<TMP_FontAsset> fonts = new();
    }
}
