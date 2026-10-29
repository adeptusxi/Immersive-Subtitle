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
        [SerializeField] private NarrativeClock clock;
        [SerializeField] private MonoBehaviour choicePresenter;
        
        public InkNarrativeRunner Runner { get; private set; }
        
        private IChoicePresenter presenter;
        private const string defaultSpeakerId = "alien1";

        private void Awake()
        {
            presenter = (IChoicePresenter)choicePresenter;
            Runner = new InkNarrativeRunner(new Story(ink.storyJson), defaultSpeakerId, SpeakerRegistry.Global);
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
            Runner.Tick(clock.Now);
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
