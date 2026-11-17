using System;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class BeamPulseCue : VisualSpeechCue
    {
        [SerializeField] private float verticalOffset = 0f;
        [SerializeField] private float forwardOffset = 0.5f;

        public static event Action<SpeakerProfile, Vector3, Vector3> BeamRequested;

        protected override SpeechCueMode Mode => SpeechCueMode.BeamPulse;

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

            Transform origin = Player.Instance.Head;
            Vector3 forward = Flat(origin.forward, FallbackForward());
            Vector3 start = origin.position + Vector3.up * verticalOffset + forward * forwardOffset;
            Vector3 target = root.position;
            target.y = start.y;

            BeamRequested?.Invoke(segment.speaker, start, target);
            Debug.Log($"[Subtitles] beam pulse for {segment.speaker.displayName}");
        }

        private static Vector3 Flat(Vector3 v, Vector3 fallback)
        {
            Vector3 flat = Vector3.ProjectOnPlane(v, Vector3.up);
            return flat.sqrMagnitude < 1e-6f ? fallback : flat.normalized;
        }

        private static Vector3 FallbackForward()
        {
            Vector3 flat = Vector3.ProjectOnPlane(Player.Instance.Head.forward, Vector3.up);
            return flat.sqrMagnitude < 1e-6f ? Vector3.forward : flat.normalized;
        }
    }
}
