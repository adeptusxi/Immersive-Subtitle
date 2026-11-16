using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using ImmersiveSubtitles.Speech;
using TMPro;
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
        [SerializeField] private ButtonUI otherTabButton;
        [SerializeField] private GameObject presentationTab;
        [SerializeField] private GameObject textTab;
        [SerializeField] private GameObject otherTab;

        [Header("Presentation Tab")]
        [SerializeField] private MultipleChoiceUI subtitleMode;
        [SerializeField] private MultipleChoiceUI indicatorMode;

        [Header("Text Tab")]
        [SerializeField] private FontCatalog fonts;
        [SerializeField] private ColorPalette palette;
        [SerializeField] private MultipleChoiceUI font;
        [SerializeField] private StepperUI size;
        [SerializeField] private SwatchUI textColor;
        [SerializeField] private SwatchUI backgroundColor;
        [SerializeField] private StepperUI backgroundOpacity;
        [SerializeField] private ToggleUI colorTextBySpeaker;
        
        [Header("Other Tab")]
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

            BindTextTab();

            presentationTabButton.Clicked += () => SelectTab(presentationTab);
            textTabButton.Clicked += () => SelectTab(textTab);
            otherTabButton.Clicked += () => SelectTab(otherTab);
            resetButton.Clicked += SubtitleRuntime.Preferences.Reset;
        }
        
        private void Start()
        {
            SelectTab(presentationTab);
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

        private void BindTextTab()
        {
            for (int i = 0; i < font.Options.Count; i++)
            {
                TMP_FontAsset asset = fonts.fonts[i];
                font.Options[i].SetLabel(asset.faceInfo.familyName, asset);
            }
            font.Bind(p => fonts.fonts.IndexOf(p.text.font != null ? p.text.font : TMP_Settings.defaultFontAsset), (p, v) => p.text.font = fonts.fonts[v]);

            size.Bind(p => p.text.sizeDegrees, (p, v) => p.text.sizeDegrees = v, 0.5f, 3f, 0.1f);
            textColor.Bind(palette, p => p.text.textColor, (p, v) => p.text.textColor = v);
            backgroundColor.Bind(palette, p => p.text.backgroundColor, (p, v) => p.text.backgroundColor = v);
            backgroundOpacity.Bind(p => p.text.backgroundOpacity, (p, v) => p.text.backgroundOpacity = v, 0f, 1f, 0.1f);
            colorTextBySpeaker.Bind(p => p.text.colorTextBySpeaker, (p, v) => p.text.colorTextBySpeaker = v);
        }

        private void SelectTab(GameObject tab)
        {
            presentationTab.SetActive(tab == presentationTab);
            textTab.SetActive(tab == textTab);
            otherTab.SetActive(tab == otherTab);
            presentationTabButton.SetSelected(tab == presentationTab);
            textTabButton.SetSelected(tab == textTab);
            otherTabButton.SetSelected(tab == otherTab);
        }
    }
}
