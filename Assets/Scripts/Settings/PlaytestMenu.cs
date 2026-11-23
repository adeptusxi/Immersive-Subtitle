using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Narrative;
using ImmersiveSubtitles.Presentation;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Settings
{
    // open/close with A, B
    [DefaultExecutionOrder(200)]
    public sealed class PlaytestMenu : MonoBehaviour
    {
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private InkNarrativeRunnerBehaviour runner; 
        [SerializeField] private ButtonUI noneButton;
        [SerializeField] private ButtonUI ringPulseButton;
        [SerializeField] private ButtonUI beamPulseButton;
        [SerializeField] private ButtonUI subtitleHudButton;
        [SerializeField] private ButtonUI dockedIndicatorButton;
        [SerializeField] private ButtonUI dockedWordButton;
        [SerializeField] private ButtonUI dockedHudButton;
        [SerializeField] private ButtonUI speechBubbleButton;

        private ButtonUI[] buttons; // index = SpeechCueMode

        public bool IsOpen => canvasRoot.activeSelf;

        private void Awake()
        {
            buttons = new[]
            {
                noneButton, 
                ringPulseButton, 
                beamPulseButton,
                subtitleHudButton, 
                dockedIndicatorButton, 
                dockedWordButton, 
                dockedHudButton,
                speechBubbleButton,
            };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                {
                    continue; 
                }

                SpeechCueMode cue = (SpeechCueMode)i;
                buttons[i].Clicked += () => Select(cue);
            }
        }

        private void OnDestroy()
        {
            SubtitleRuntime.Preferences.Changed -= Refresh;
        }

        private void Start()
        {
            canvasRoot.SetActive(false);

            SubtitleRuntime.Preferences.Changed += Refresh;
            Refresh();
        }

        private void LateUpdate()
        {
            if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Two))
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

        private void Select(SpeechCueMode cue)
        {
            SubtitleRuntime.Preferences.Current.speechCueMode = cue;
            SubtitleRuntime.Preferences.NotifyChanged();
            runner.UseInkFor(cue);
        }

        private void Refresh()
        {
            int selected = (int)SubtitleRuntime.Preferences.Current.speechCueMode;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].SetSelected(i == selected);
                }
            }
        }
    }
}
