using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Speech
{
    [DefaultExecutionOrder(0)]
    public sealed class VoicePlayer : MonoBehaviour
    {
        private enum State { Idle, Speaking, Resting, Stopping }

        [SerializeField] private Speaker speaker;
        [SerializeField] private NarrativeClock clock;
        [SerializeField] private AudioSource source;

        private State state;
        private Voice voice;
        private string currentTurn;
        private double segmentStart;
        private double segmentEnd;

        private void Awake()
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialize = true;
            source.spatialBlend = 1f;
            source.volume = 0f;
        }

        private void Start()
        {
            speaker.Queue.SegmentStarted += OnSegmentStarted;
            speaker.Queue.SegmentEnded += OnSegmentEnded;
            clock.PausedChanged += OnPausedChanged;
        }

        private void OnDestroy()
        {
            speaker.Queue.SegmentStarted -= OnSegmentStarted;
            speaker.Queue.SegmentEnded -= OnSegmentEnded;
            clock.PausedChanged -= OnPausedChanged;
        }

        private void Update()
        {
            if (clock.IsPaused)
                return;

            if (state == State.Speaking)
            {
                // freezes when paused and hits 0 exactly at planned end 
                double now = clock.Now;
                double fadeIn = voice.fadeInSeconds > 0f ? (now - segmentStart) / voice.fadeInSeconds : 1.0;
                double fadeOut = voice.fadeOutSeconds > 0f ? (segmentEnd - now) / voice.fadeOutSeconds : 1.0;
                source.volume = voice.volume * Mathf.Clamp01((float)System.Math.Min(fadeIn, fadeOut));
            }
            else if (state == State.Stopping)
            {
                float step = voice.fadeOutSeconds > 0f ? voice.volume * Time.unscaledDeltaTime / voice.fadeOutSeconds : 1f;
                source.volume = Mathf.Max(0f, source.volume - step);
                if (source.volume <= 0f)
                    StopNow();
            }
        }

        private void OnSegmentStarted(SegmentInstance segment)
        {
            Voice segmentVoice = speaker.Queue.Profile.voice;
            if (segmentVoice == null || segmentVoice.sourceClips == null || segmentVoice.sourceClips.Count == 0)
                return;

            if (segment.turnId != currentTurn)
            {
                AudioClip clip = segmentVoice.sourceClips[Random.Range(0, segmentVoice.sourceClips.Count)];
                if (clip == null)
                    return;

                currentTurn = segment.turnId;
                source.clip = clip;
                source.pitch = Random.Range(segmentVoice.pitchRange.x, segmentVoice.pitchRange.y);
                source.volume = 0f;
                source.Play();
                source.timeSamples = Random.Range(0, clip.samples);
            }
            else
            {
                source.UnPause();
            }

            voice = segmentVoice;
            segmentStart = segment.startedAt;
            segmentEnd = segment.startedAt + segment.plannedDuration;
            state = State.Speaking;

            if (clock.IsPaused)
                source.Pause();
        }

        private void OnSegmentEnded(SegmentInstance segment, SegmentEndReason reason)
        {
            if (state != State.Speaking)
                return;

            if (reason == SegmentEndReason.Finished)
            {
                source.volume = 0f;
                source.Pause();
                state = State.Resting;
                return;
            }

            currentTurn = null;
            if (clock.IsPaused)
                StopNow();
            else
                state = State.Stopping;
        }

        private void OnPausedChanged(bool paused)
        {
            if (paused)
            {
                if (state == State.Speaking)
                    source.Pause();
                else if (state == State.Stopping)
                    StopNow();
            }
            else if (state == State.Speaking)
            {
                source.UnPause();
            }
        }

        private void StopNow()
        {
            source.Stop();
            source.volume = 0f;
            state = State.Idle;
        }
    }
}
