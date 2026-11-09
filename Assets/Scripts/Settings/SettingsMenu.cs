using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Settings
{
    [DefaultExecutionOrder(200)]
    public sealed class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private SubtitlePreferences defaults = new();
        [SerializeField] private ButtonUI resetButton;

        [Header("Tabs")]
        [SerializeField] private ButtonUI presentationTabButton;
        [SerializeField] private ButtonUI textTabButton;
        [SerializeField] private GameObject presentationTab;
        [SerializeField] private GameObject textTab;

        [Header("Presentation Tab")]
        [SerializeField] private MultipleChoiceUI subtitleMode;
        [SerializeField] private MultipleChoiceUI indicatorMode;
        [SerializeField] private ToggleUI keepTextDuringGaps;
        [SerializeField] private MultipleChoiceUI entranceStyle;
        [SerializeField] private ToggleUI facePlayer;
        [SerializeField] private MultipleChoiceUI sizeMode;
        [SerializeField] private ToggleUI lagFollow;

        public bool IsOpen => canvasRoot.activeSelf;

        private void Awake()
        {
            SubtitleRuntime.Preferences.SetDefaults(defaults);

            subtitleMode.Bind(p => (int)p.mode, (p, v) => p.mode = (PresentationMode)v);
            indicatorMode.Bind(p => (int)p.speechCueMode, (p, v) => p.speechCueMode = (SpeechCueMode)v);
            keepTextDuringGaps.Bind(p => p.keepTextDuringGaps, (p, v) => p.keepTextDuringGaps = v);
            entranceStyle.Bind(p => (int)p.entranceStyle, (p, v) => p.entranceStyle = (EntranceStyle)v);
            facePlayer.Bind(p => p.facePlayer, (p, v) => p.facePlayer = v);
            sizeMode.Bind(p => (int)p.sizeMode, (p, v) => p.sizeMode = (SubtitleSizeMode)v);
            lagFollow.Bind(p => p.lagFollow, (p, v) => p.lagFollow = v);

            presentationTabButton.Clicked += () => SelectTab(true);
            textTabButton.Clicked += () => SelectTab(false);
            resetButton.Clicked += SubtitleRuntime.Preferences.Reset;
        }
        
        private void Start()
        {
            SelectTab(true);
            canvasRoot.SetActive(false);
        }

        private void LateUpdate()
        {
            if (OVRInput.GetDown(OVRInput.Button.Start))
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

        private void SelectTab(bool presentation)
        {
            presentationTab.SetActive(presentation);
            textTab.SetActive(!presentation);
            presentationTabButton.SetSelected(presentation);
            textTabButton.SetSelected(!presentation);
        }
    }
}
