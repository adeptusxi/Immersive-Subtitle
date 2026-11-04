using System;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using UnityEngine;

namespace ImmersiveSubtitles.Settings
{
    public sealed class ToggleUI : MonoBehaviour
    {
        [SerializeField] private ButtonUI button;

        private Func<SubtitlePreferences, bool> getter;
        private Action<SubtitlePreferences, bool> setter;

        public void Bind(Func<SubtitlePreferences, bool> get, Action<SubtitlePreferences, bool> set)
        {
            getter = get;
            setter = set;

            button.Clicked += HandleClicked;
            SubtitleRuntime.Preferences.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            SubtitleRuntime.Preferences.Changed -= Refresh;
        }

        private void HandleClicked()
        {
            SubtitlePreferences current = SubtitleRuntime.Preferences.Current;
            setter(current, !getter(current));
            SubtitleRuntime.Preferences.NotifyChanged();
        }

        private void Refresh()
        {
            button.SetSelected(getter(SubtitleRuntime.Preferences.Current));
        }
    }
}
