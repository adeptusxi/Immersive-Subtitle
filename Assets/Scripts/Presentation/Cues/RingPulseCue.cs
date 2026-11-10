using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class RingPulseCue : VisualSpeechCue
    {
        [SerializeField] private ParticleSystem pulseVFX;

        [Header("Light")]
        [SerializeField] private Light pointLight;
        [SerializeField, Tooltip("meters behind head")] private float headOffset = 0.3f;
        [SerializeField] private float minIntensity = 0.75f;
        [SerializeField] private float maxIntensity = 1.25f;
        [SerializeField] private float lightPulseFrequency = 3f; // average 
        [SerializeField] private float lightFadeSeconds = 0.5f;
        
        protected override SpeechCueMode Mode => SpeechCueMode.RingPulse;

        private const float pulseLifetime = 5f;
        private const float intensitySmoothing = 15f;

        private class HeadLight
        {
            public Light light;
            public SpeakerProfile speaker;
            public SpeakerQueue queue;
            public Transform root;
            public bool showing;
            public bool fading;
            public float fadeFrom;
            public float phase;
            public float time;
            public float seed;
        }

        private readonly List<HeadLight> lights = new();
        private double lastNarrativeNow;
        
        #region Unity

        protected override void OnEnable()
        {
            base.OnEnable();
            SubtitleBus.SegmentStarted += HandleSegmentStarted;
            SubtitleBus.SegmentEnded += HandleSegmentEnded;
            lastNarrativeNow = SubtitleRuntime.ActiveSubtitles.Now;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SubtitleBus.SegmentStarted -= HandleSegmentStarted;
            SubtitleBus.SegmentEnded -= HandleSegmentEnded;
            foreach (HeadLight headLight in lights)
            {
                Destroy(headLight.light.gameObject);
            }

            lights.Clear();
        }
        
        private void Update()
        {
            double narrativeNow = SubtitleRuntime.ActiveSubtitles.Now;
            float narrativeDelta = (float)(narrativeNow - lastNarrativeNow);
            lastNarrativeNow = narrativeNow;
            float delta = Time.unscaledDeltaTime;

            for (int i = lights.Count - 1; i >= 0; i--)
            {
                HeadLight headLight = lights[i];
                if (!headLight.fading && (headLight.queue.IsIdle || !IsOutOfView(headLight.root, true)))
                {
                    StartFade(headLight);
                }

                if (headLight.fading)
                {
                    float step = lightFadeSeconds > 0f ? headLight.fadeFrom / lightFadeSeconds * delta : float.MaxValue;
                    headLight.light.intensity = Mathf.Max(0f, headLight.light.intensity - step);
                    if (headLight.light.intensity <= 0f)
                    {
                        Destroy(headLight.light.gameObject);
                        lights.RemoveAt(i);
                    }

                    continue;
                }

                float target = minIntensity;
                if (headLight.showing)
                {
                    target = SampleWave(headLight, narrativeDelta);
                }

                float blend = 1f - Mathf.Exp(-intensitySmoothing * delta);
                headLight.light.intensity = Mathf.Lerp(headLight.light.intensity, target, blend);
            }
        }
        
        #endregion 

        protected override void HideCue()
        {
            foreach (HeadLight headLight in lights)
            {
                StartFade(headLight);
            }
        }

        private void HandleSegmentStarted(SegmentInstance segment)
        {
            HeadLight existing = GetCurrentLight(segment.speaker);
            if (existing != null)
            {
                existing.showing = true;
            }

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

            if (existing != null)
            {
                return;
            }

            SpeakerRegistry.Global.TryGet(segment.speaker.id, out SpeakerQueue queue);
            Light light = Instantiate(pointLight, Player.Instance.Head);
            light.transform.localPosition = Vector3.back * headOffset;
            light.transform.localRotation = Quaternion.identity;
            light.color = segment.speaker.color;
            light.intensity = 0f;
            lights.Add(new HeadLight
            {
                light = light,
                speaker = segment.speaker,
                queue = queue,
                root = root,
                showing = true,
                seed = Random.Range(0f, 100f),
            });
        }

        private void HandleSegmentEnded(SegmentInstance segment, SegmentEndReason reason)
        {
            HeadLight headLight = GetCurrentLight(segment.speaker);
            if (headLight != null)
            {
                headLight.showing = false;
            }
        }

        // noisy sine wave 
        private float SampleWave(HeadLight headLight, float narrativeDelta)
        {
            headLight.time += narrativeDelta;
            float speed = Mathf.Lerp(0.6f, 1.4f, Mathf.PerlinNoise(headLight.seed, headLight.time));
            headLight.phase += narrativeDelta * 2f * Mathf.PI * lightPulseFrequency * speed;
            float depth = Mathf.Lerp(0.4f, 1f, Mathf.PerlinNoise(headLight.seed + 17f, headLight.time * 1.3f));
            float wave = depth * (0.5f + 0.5f * Mathf.Sin(headLight.phase));
            return Mathf.Lerp(minIntensity, maxIntensity, wave);
        }

        private static void StartFade(HeadLight headLight)
        {
            if (headLight.fading)
            {
                return;
            }

            headLight.fading = true;
            headLight.fadeFrom = headLight.light.intensity;
        }

        private HeadLight GetCurrentLight(SpeakerProfile speaker)
        {
            foreach (HeadLight headLight in lights)
            {
                if (headLight.speaker == speaker && !headLight.fading)
                {
                    return headLight;
                }
            }

            return null;
        }
    }
}
