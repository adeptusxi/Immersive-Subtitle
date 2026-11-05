using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    [DefaultExecutionOrder(200)]
    public abstract class VisualSpeechCue : MonoBehaviour
    {
        [SerializeField] private float fovMarginDegrees = 3f;

        private bool wasActive;

        protected abstract SpeechCueMode Mode { get; }

        protected bool IsActive
        {
            get
            {
                SubtitlePreferences preferences = SubtitleRuntime.Preferences.Current;
                return preferences.speechCueMode == Mode && preferences.mode != PresentationMode.Group;
            }
        }

        protected virtual void OnEnable()
        {
            SubtitleRuntime.Preferences.Changed += HandlePreferencesChanged;
            HandlePreferencesChanged();
        }

        protected virtual void OnDisable()
        {
            SubtitleRuntime.Preferences.Changed -= HandlePreferencesChanged;
            if (wasActive)
            {
                wasActive = false;
                HideCue();
            }
        }

        private void LateUpdate()
        {
            if (IsActive)
            {
                UpdateCue();
            }
        }

        protected virtual void UpdateCue() { }

        protected virtual void ApplyPreferences(SubtitlePreferences preferences) { }

        protected virtual void HideCue() { }

        protected bool IsOutOfView(Transform root, bool wasOutOfView)
        {
            float margin = wasOutOfView ? -fovMarginDegrees : 0f;
            return !Player.Instance.IsInFOV(root.position, margin);
        }

        private void HandlePreferencesChanged()
        {
            bool active = IsActive;
            if (wasActive && !active)
            {
                HideCue();
            }

            wasActive = active;
            if (active)
            {
                ApplyPreferences(SubtitleRuntime.Preferences.Current);
            }
        }
    }
}
