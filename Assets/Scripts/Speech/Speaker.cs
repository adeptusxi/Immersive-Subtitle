using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    public sealed class Speaker : MonoBehaviour
    {
        [SerializeField] private SpeakerProfile profile;
        [SerializeField] private NarrativeClock clock;
        [SerializeField] private TimingProfile timing;
        
        public SpeakerQueue Queue { get; private set; }

        private void Awake()
        {
            Queue = new SpeakerQueue(profile, timing);
        }

        private void OnEnable()
        {
            SpeakerRegistry.Global.Register(Queue);
        }

        private void OnDisable()
        {
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
    }
}
