using System.Collections.Generic;
using System.Linq;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using Ink.Runtime;
using Ink.UnityIntegration;
using UnityEngine;

namespace ImmersiveSubtitles.Narrative
{
    [DefaultExecutionOrder(100)]
    public sealed class InkNarrativeRunnerBehaviour : MonoBehaviour
    {
        [SerializeField] private InkFile ink;
        [SerializeField] private InkFile altInk;
        [SerializeField] private SpeechCueMode[] altInkCues = { SpeechCueMode.DockedWord, SpeechCueMode.DockedHUD };
        [SerializeField] private MonoBehaviour choicePresenter;
        [SerializeField] private EmotionCatalog emotions;
        [SerializeField] private string restartKnot = "replay";

        public InkNarrativeRunner Runner { get; private set; }

        private IChoicePresenter presenter;
        private bool usingAlt;
        private const string defaultSpeakerId = "alien1";

        private void Awake()
        {
            presenter = (IChoicePresenter)choicePresenter;
            if (emotions != null)
            {
                emotions.RegisterFonts();
            }

            CreateRunner(WantsAlt());
            SubtitleRuntime.Preferences.Changed += HandlePreferencesChanged;
        }

        private void OnDestroy()
        {
            SubtitleRuntime.Preferences.Changed -= HandlePreferencesChanged;
            Runner.ChoicesPresented -= HandleChoicesPresented;
        }

        private void Start()
        {
            Runner.Begin();
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.Four))
            {
                Restart();
            }

            if (!NarrativeClock.Instance.IsPaused)
            {
                Runner.Tick(NarrativeClock.Instance.Now);
            }
        }

        public void Restart()
        {
            presenter?.Hide();
            Runner.Restart();
        }

        private bool WantsAlt()
        {
            return altInkCues.Contains(SubtitleRuntime.Preferences.Current.speechCueMode);
        }

        private void CreateRunner(bool alt)
        {
            usingAlt = alt;
            InkFile file = alt ? altInk : ink;
            Runner = new InkNarrativeRunner(new Story(file.storyJson), defaultSpeakerId, SpeakerRegistry.Global, emotions, restartKnot);
            Runner.ChoicesPresented += HandleChoicesPresented;
        }

        // swap to the other ink file and jump to its restart knot 
        private void HandlePreferencesChanged()
        {
            bool alt = WantsAlt();
            if (alt == usingAlt)
            {
                return;
            }

            bool started = Runner.State != RunnerState.NotStarted;
            if (started)
            {
                Restart(); 
            }
            Runner.ChoicesPresented -= HandleChoicesPresented;

            CreateRunner(alt);
            if (started)
            {
                Runner.Begin();
                Runner.Restart();
            }
        }

        private void HandleChoicesPresented(IReadOnlyList<ChoiceOption> options)
        {
            if (presenter == null)
            {
                return;
            }

            presenter.Show(options, i =>
            {
                presenter.Hide();
                Runner.SelectChoice(i);
            });
        }
    }
}
