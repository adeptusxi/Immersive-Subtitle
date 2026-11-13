using System;
using System.Globalization;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Settings
{
    public sealed class StepperUI : MonoBehaviour
    {
        [SerializeField] private ButtonUI decrease;
        [SerializeField] private ButtonUI increase;
        [SerializeField] private TMP_Text valueText;

        private Func<SubtitlePreferences, float> getter;
        private Action<SubtitlePreferences, float> setter;
        private float min;
        private float max;
        private float step;
        private string format;

        public void Bind(Func<SubtitlePreferences, float> get, Action<SubtitlePreferences, float> set, float min, float max, float step, string format = "0.0#")
        {
            getter = get;
            setter = set;
            this.min = min;
            this.max = max;
            this.step = step;
            this.format = format;

            decrease.Clicked += () => Step(-1);
            increase.Clicked += () => Step(1);
            SubtitleRuntime.Preferences.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            SubtitleRuntime.Preferences.Changed -= Refresh;
        }

        private void Step(int direction)
        {
            SubtitlePreferences current = SubtitleRuntime.Preferences.Current;
            float snapped = Mathf.Round(getter(current) / step) * step;
            setter(current, Mathf.Clamp(snapped + direction * step, min, max));
            SubtitleRuntime.Preferences.NotifyChanged();
        }

        private void Refresh()
        {
            float value = getter(SubtitleRuntime.Preferences.Current);
            valueText.text = value.ToString(format, CultureInfo.InvariantCulture);
            decrease.GetComponent<Button>().interactable = value > min + step * 0.5f;
            increase.GetComponent<Button>().interactable = value < max - step * 0.5f;
        }
    }
}
