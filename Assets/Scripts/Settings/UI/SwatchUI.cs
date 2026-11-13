using System;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Settings
{
    public sealed class SwatchUI : MonoBehaviour
    {
        [SerializeField] private ButtonUI[] swatches;

        private ColorPalette palette;
        private Func<SubtitlePreferences, Color> getter;
        private Action<SubtitlePreferences, Color> setter;

        public void Bind(ColorPalette palette, Func<SubtitlePreferences, Color> get, Action<SubtitlePreferences, Color> set)
        {
            this.palette = palette;
            getter = get;
            setter = set;

            for (int i = 0; i < swatches.Length; i++)
            {
                Color color = palette.colors[i].color;
                swatches[i].GetComponent<Image>().color = color;
                swatches[i].Clicked += () => Select(color);
            }

            SubtitleRuntime.Preferences.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            SubtitleRuntime.Preferences.Changed -= Refresh;
        }

        private void Select(Color color)
        {
            setter(SubtitleRuntime.Preferences.Current, color);
            SubtitleRuntime.Preferences.NotifyChanged();
        }

        private void Refresh()
        {
            Color selected = getter(SubtitleRuntime.Preferences.Current);
            for (int i = 0; i < swatches.Length; i++)
            {
                swatches[i].SetSelected(palette.colors[i].color == selected);
            }
        }
    }
}
