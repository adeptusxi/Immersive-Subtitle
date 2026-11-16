using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    public class Speaker : MonoBehaviour
    {
        [SerializeField] protected SpeakerProfile profile;
        [SerializeField] private TimingProfile timing;
        [SerializeField] protected Transform speakerRoot;
        [SerializeField] private Transform dockTarget;

        public SpeakerQueue Queue { get; private set; }
        protected Transform SpeakerRoot => speakerRoot;

        private void Awake()
        {
            Queue = new SpeakerQueue(profile, timing);
        }

        private void OnEnable()
        {
            SpeakerRegistry.Global.Register(Queue, speakerRoot, dockTarget);
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
