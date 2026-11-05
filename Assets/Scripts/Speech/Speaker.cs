using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    public sealed class Speaker : MonoBehaviour
    {
        [SerializeField] private SpeakerProfile profile;
        [SerializeField] private TimingProfile timing;
        [SerializeField] private Transform speakerRoot;

        public SpeakerQueue Queue { get; private set; }

        private void Awake()
        {
            Queue = new SpeakerQueue(profile, timing);
        }

        private void OnEnable()
        {
            SpeakerRegistry.Global.Register(Queue, speakerRoot);
        }

        private void OnDisable()
        {
            Queue.Stop();
            SpeakerRegistry.Global.Unregister(Queue);
        }

        private void Update()
        {
            Queue.Tick(NarrativeClock.Instance.Now);
        }

        public void Stop()
        {
            Queue.Stop();
        }
    }
}
