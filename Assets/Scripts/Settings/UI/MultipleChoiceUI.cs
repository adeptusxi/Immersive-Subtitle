using System;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using UnityEngine;

namespace ImmersiveSubtitles.Settings
{
    public sealed class MultipleChoiceUI : MonoBehaviour
    {
        private ButtonUI[] options;

        private Func<SubtitlePreferences, int> getter;
        private Action<SubtitlePreferences, int> setter;

        public void Bind(Func<SubtitlePreferences, int> get, Action<SubtitlePreferences, int> set)
        {
            getter = get;
            setter = set;

            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                options[i].Clicked += () => Select(index);
            }

            SubtitleRuntime.Preferences.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            SubtitleRuntime.Preferences.Changed -= Refresh;
        }

        private void Select(int index)
        {
            setter(SubtitleRuntime.Preferences.Current, index);
            SubtitleRuntime.Preferences.NotifyChanged();
        }

        private void Refresh()
        {
            int selected = getter(SubtitleRuntime.Preferences.Current);
            for (int i = 0; i < options.Length; i++)
            {
                options[i].SetSelected(i == selected);
            }
        }
    }
}
