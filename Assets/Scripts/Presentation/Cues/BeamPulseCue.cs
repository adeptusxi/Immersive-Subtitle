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
        [SerializeField] private float leadInSeconds = 0.4f;

        public static event Action<SpeakerProfile, Vector3, Vector3> BeamRequested;

        protected override SpeechCueMode Mode => SpeechCueMode.BeamPulse;

        protected override void OnEnable()
        {
            base.OnEnable();
            SubtitleBus.SegmentPrerolling += HandleSegmentPrerolling;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SubtitleBus.SegmentPrerolling -= HandleSegmentPrerolling;
        }

        private void HandleSegmentPrerolling(SegmentPreroll preroll)
        {
            if (!IsActive || preroll.indexInTurn != 0)
            {
                return;
            }

            Transform root = SpeakerRegistry.Global.RootOf(preroll.speaker);
            if (root == null || Player.Instance.IsInFOV(root.position))
            {
                return;
            }

            Transform head = Player.Instance.Head;
            Vector3 start = head.position + head.forward * forwardOffset + head.up * verticalOffset;
            Vector3 target = root.position;
            target.y = head.position.y + verticalOffset;

            BeamRequested?.Invoke(preroll.speaker, start, target);
            preroll.Request(leadInSeconds);
            Debug.Log($"[Subtitles] beam pulse for {preroll.speaker.displayName}");
        }
    }
}
