using EasyTextEffects.Effects;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [CreateAssetMenu(menuName = "Immersive Subtitles/Emotion Style")]
    public sealed class EmotionStyle : ScriptableObject
    {
        [Tooltip("same tag used in ink")] public string id;
        [Tooltip("optional")] public TMP_FontAsset font;
        [Tooltip("optional")] public TextEffectInstance effect;
        public Color color = Color.white;
    }
}
