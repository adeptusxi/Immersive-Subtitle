using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [CreateAssetMenu(menuName = "Immersive Subtitles/Emotion Catalog")]
    public sealed class EmotionCatalog : ScriptableObject
    {
        public List<EmotionStyle> emotions = new();

        public bool TryGet(string id, out EmotionStyle emotion)
        {
            foreach (EmotionStyle candidate in emotions)
            {
                if (string.Equals(candidate.id, id, StringComparison.OrdinalIgnoreCase))
                {
                    emotion = candidate;
                    return true;
                }
            }

            emotion = null;
            return false;
        }
        
        public void RegisterFonts()
        {
            foreach (EmotionStyle emotion in emotions)
            {
                if (emotion.font != null)
                {
                    MaterialReferenceManager.AddFontAsset(emotion.font);
                }
            }
        }
    }
}
