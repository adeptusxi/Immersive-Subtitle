using System.Collections.Generic;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [CreateAssetMenu(menuName = "Immersive Subtitles/Voice")]
    public sealed class Voice : ScriptableObject
    {
        [Tooltip("import as streaming or compressed in memory, not decompress on load")] public List<AudioClip> sourceClips;
        public float fadeInSeconds = 0.03f;
        public float fadeOutSeconds = 0.08f;
        public Vector2 pitchRange = new(1f, 1f);
        public float volume = 1f;
    }
}
