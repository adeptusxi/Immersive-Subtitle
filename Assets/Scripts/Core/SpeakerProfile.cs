using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    [CreateAssetMenu(menuName = "Immersive Subtitles/Speaker Profile")]
    public sealed class SpeakerProfile : ScriptableObject
    {
        public string id;
        public string displayName;
        public Color color = Color.white;
        public float speechRate = 1f;
        [Tooltip("null for no audio")] public Voice voice;
        public bool overrideTextStyle;
        public TextStylePreferences textStyle = new();
        
        private void OnValidate()
        {
            speechRate = Mathf.Max(0.1f, speechRate);
        }
    }
}
