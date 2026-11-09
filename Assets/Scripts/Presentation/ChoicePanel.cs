using System;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class ChoicePanel : MonoBehaviour, IChoicePresenter
    {
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private ButtonUI[] buttons;

        public static ChoicePanel Instance { get; private set; }

        private readonly HashSet<object> suspenders = new();
        private IReadOnlyList<ChoiceOption> options;
        private Action<int> pending;

        private void Awake()
        {
            Instance = this;

            for (int i = 0; i < buttons.Length; i++)
            {
                int slot = i;
                buttons[i].Clicked += () => Choose(slot);
            }

            canvasRoot.SetActive(false);
        }

        public void Show(IReadOnlyList<ChoiceOption> options, Action<int> onChosen)
        {
            this.options = options;
            pending = onChosen;

            if (options.Count > buttons.Length)
            {
                Debug.LogWarning($"[Subtitles] choice has {options.Count} options but the panel has {buttons.Length} buttons");
            }

            TMP_FontAsset font = SubtitleRuntime.Preferences.Current.text.font;
            if (font == null)
            {
                font = TMP_Settings.defaultFontAsset;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                bool used = i < options.Count;
                buttons[i].gameObject.SetActive(used);
                if (used)
                {
                    buttons[i].SetLabel(options[i].label, font);
                }
            }

            canvasRoot.SetActive(suspenders.Count == 0);
        }

        public void Hide()
        {
            pending = null;
            canvasRoot.SetActive(false);
        }

        // hides while maintaining pending choice
        public void SetSuspended(object owner, bool suspended)
        {
            if (suspended)
            {
                suspenders.Add(owner);
            }
            else
            {
                suspenders.Remove(owner);
            }

            canvasRoot.SetActive(suspenders.Count == 0 && pending != null);
        }

        private void Choose(int slot)
        {
            if (pending == null)
            {
                return; // ignore double click
            }

            Action<int> callback = pending;
            pending = null;
            callback(options[slot].index);
        }
    }
}
