using System;
using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Narrative
{
    public sealed class DemoInkBindings : MonoBehaviour
    {
        [SerializeField] private InkNarrativeRunnerBehaviour runner;

        private void Start()
        {
            runner.Runner.BindExternalFunction("set_presentation_mode", SetMode);
        }

        private void SetMode(string modeName)
        {
            if (!Enum.TryParse(modeName, true, out PresentationMode mode))
            {
                Debug.LogWarning($"[Subtitles] unknown presentation mode '{modeName}'");
                return;
            }

            SubtitleRuntime.Preferences.Current.mode = mode;
            SubtitleRuntime.Preferences.NotifyChanged();
            Debug.Log($"[Subtitles] presentation mode {mode}");
        }
    }
}
