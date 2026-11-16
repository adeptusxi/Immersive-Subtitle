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
        [SerializeField] private MonoBehaviour choicePresenter;
        [SerializeField] private EmotionCatalog emotions;
        [SerializeField] private string restartKnot = "replay";
        
        public InkNarrativeRunner Runner { get; private set; }
        
        private IChoicePresenter presenter;
        private const string defaultSpeakerId = "alien1";

        private void Awake()
        {
            presenter = (IChoicePresenter)choicePresenter;
            if (emotions != null)
            {
                emotions.RegisterFonts();
            }

            Runner = new InkNarrativeRunner(new Story(ink.storyJson), defaultSpeakerId, SpeakerRegistry.Global, emotions, restartKnot);
            Runner.ChoicesPresented += HandleChoicesPresented;
        }

        private void OnDestroy()
        {
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
