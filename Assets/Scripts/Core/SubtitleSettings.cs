using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [CreateAssetMenu(menuName = "Immersive Subtitles/Subtitle Settings")]
    public sealed class SubtitleSettings : ScriptableObject
    {
        public string defaultSpeakerId = "alien1";
        public string fallbackSpeakerId = "alien1";
        public bool choicesWaitForAsync = true;
    }
}
