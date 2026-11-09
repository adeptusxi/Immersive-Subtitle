using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using ImmersiveSubtitles.Speech;
using TMPro;
using UnityEngine;

namespace ImmersiveSubtitles.Settings
{
    // open/close with A, B, X, Y
    [DefaultExecutionOrder(200)]
    public sealed class PlaytestMenu : MonoBehaviour
    {
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private ButtonUI pulseButton;
        [SerializeField] private ButtonUI dockedButton;
        [SerializeField] private ButtonUI hudButton;
        [SerializeField] private ButtonUI laggedHudButton;
        [SerializeField] private TMP_Text statusText;

        private static readonly string[] Labels = { "A: Pulse", "B: Docked", "C: HUD", "D: lagged HUD" };
        private ButtonUI[] buttons;

        public bool IsOpen => canvasRoot.activeSelf;

        private void Awake()
        {
            buttons = new[] { pulseButton, dockedButton, hudButton, laggedHudButton };
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[i].Clicked += () => Select(index);
            }

            SubtitleRuntime.Preferences.Changed += Refresh;
        }

        private void OnDestroy()
        {
            SubtitleRuntime.Preferences.Changed -= Refresh;
        }

        private void Start()
        {
            canvasRoot.SetActive(false);
            Refresh();
        }

        private void LateUpdate()
        {
            if (OVRInput.GetDown(OVRInput.Button.One) 
                || OVRInput.GetDown(OVRInput.Button.Two)
                || OVRInput.GetDown(OVRInput.Button.Three) 
                || OVRInput.GetDown(OVRInput.Button.Four))
            {
                Toggle();
            }
        }

        public void Open()
        {
            canvasRoot.SetActive(true);
            NarrativeClock.Instance.Pause(this);
            ChoicePanel.Instance.SetSuspended(this, true);
        }

        public void Close()
        {
            canvasRoot.SetActive(false);
            NarrativeClock.Instance.Resume(this);
            ChoicePanel.Instance.SetSuspended(this, false);
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void Select(int index)
        {
            SubtitlePreferences prefs = SubtitleRuntime.Preferences.Current;
            switch (index)
            {
                case 0:
                    prefs.speechCueMode = SpeechCueMode.RingPulse;
                    break;
                case 1:
                    prefs.speechCueMode = SpeechCueMode.DockedIndicator;
                    break;
                case 2:
                    prefs.speechCueMode = SpeechCueMode.SubtitleHUD;
                    prefs.lagFollow = false;
                    break;
                case 3:
                    prefs.speechCueMode = SpeechCueMode.SubtitleHUD;
                    prefs.lagFollow = true;
                    break;
            }

            SubtitleRuntime.Preferences.NotifyChanged();
            Close();
        }

        private void Refresh()
        {
            int match = PrefToIndex(SubtitleRuntime.Preferences.Current);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetSelected(i == match);
            }

            statusText.text = match >= 0 ? Labels[match] : "---";
        }

        private static int PrefToIndex(SubtitlePreferences prefs)
        {
            return prefs.speechCueMode switch
            {
                SpeechCueMode.RingPulse => 0,
                SpeechCueMode.DockedIndicator => 1,
                SpeechCueMode.SubtitleHUD => prefs.lagFollow ? 3 : 2,
                _ => -1,
            };
        }
    }
}
