using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    public sealed class Speaker : MonoBehaviour
    {
        [SerializeField] private SpeakerProfile profile;
        [SerializeField] private NarrativeClock clock;
        [SerializeField] private TimingProfile timing;
        
        [Header("Out-Of-View Speech Cues")]
        [SerializeField] private Transform speakerRoot;
        [SerializeField] private Color speakerColor;
        [SerializeField] private ParticleSystem pulseVFX;
        private const float pulseVFXLifetime = 5f;

        private string currentTurnId;

        public SpeakerQueue Queue { get; private set; }

        private void Awake()
        {
            Queue = new SpeakerQueue(profile, timing);
        }

        private void OnEnable()
        {
            Queue.SegmentStarted += OnSegmentStarted;
            SpeakerRegistry.Global.Register(Queue);
        }

        private void OnDisable()
        {
            Queue.SegmentStarted -= OnSegmentStarted;
            Queue.Stop();
            SpeakerRegistry.Global.Unregister(Queue);
        }

        private void Update()
        {
            Queue.Tick(clock.Now);
        }
        
        public void Stop()
        {
            Queue.Stop();
        }

        private void OnSegmentStarted(SegmentInstance segment)
        {
            if (segment.turnId == currentTurnId)
                return;
            currentTurnId = segment.turnId;

            if (pulseVFX == null || speakerRoot == null)
                return;

            if (Player.Instance.IsInFOV(speakerRoot.position))
            {
                return;
            }

            ParticleSystem pulse = Instantiate(pulseVFX, speakerRoot);
            ParticleSystem.MainModule main = pulse.main;
            main.startColor = speakerColor;
            Destroy(pulse.gameObject, pulseVFXLifetime);
        }
    }
}
