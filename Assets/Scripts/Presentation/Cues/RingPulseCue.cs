using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class RingPulseCue : VisualSpeechCue
    {
        [SerializeField] private ParticleSystem pulseVFX;

        protected override SpeechCueMode Mode => SpeechCueMode.RingPulse;
        
        private const float pulseLifetime = 5f;

        protected override void OnEnable()
        {
            base.OnEnable();
            SubtitleBus.SegmentStarted += HandleSegmentStarted;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SubtitleBus.SegmentStarted -= HandleSegmentStarted;
        }

        private void HandleSegmentStarted(SegmentInstance segment)
        {
            if (!IsActive || segment.indexInTurn != 0)
            {
                return;
            }

            Transform root = SpeakerRegistry.Global.RootOf(segment.speaker);
            if (root == null || Player.Instance.IsInFOV(root.position))
            {
                return;
            }

            ParticleSystem pulse = Instantiate(pulseVFX, root);
            ParticleSystem.MainModule main = pulse.main;
            main.startColor = segment.speaker.color;
            Destroy(pulse.gameObject, pulseLifetime);
        }
    }
}
