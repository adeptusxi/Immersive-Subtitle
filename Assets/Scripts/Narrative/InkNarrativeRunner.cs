using System;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using Ink;
using Ink.Runtime;
using UnityEngine;

namespace ImmersiveSubtitles.Narrative
{
    // runs ink story and handles ink<->game interaction 
    public sealed class InkNarrativeRunner
    {
        private const int MaxIterationsPerTick = 500;

        private sealed class Op
        {
            public Action start; // invokved when op reaches front of queue 
            public Func<bool> isDone; // null means it completes instantly 
            public bool started;
        }

        private readonly Story story;
        private readonly SpeakerRegistry speakers;
        private readonly TurnTracker turnTracker = new();
        private readonly Queue<Op> ops = new();
        private readonly List<ChoiceOption> pendingChoices = new();
        private double now; // time 
        private string lastSpeakerId;

        public RunnerState State { get; private set; } = RunnerState.NotStarted;
        public IReadOnlyList<ChoiceOption> PendingChoices => pendingChoices;
        public event Action<IReadOnlyList<ChoiceOption>> ChoicesPresented;

        public InkNarrativeRunner(Story story, string defaultSpeakerId, SpeakerRegistry speakers)
        {
            this.story = story;
            this.speakers = speakers;
            lastSpeakerId = defaultSpeakerId;

            story.allowExternalFunctionFallbacks = false;
            story.onError += (message, type) =>
            {
                if (type == ErrorType.Error)
                {
                    Debug.LogError($"[Subtitles] ink: {message}");
                }
                else
                {
                    Debug.LogWarning($"[Subtitles] ink: {message}");
                }
            };
            
            BindExternals();
        }

        public void Begin()
        {
            State = RunnerState.Running;
        }

        // VAR variables in ink file 
        public void SetVariable(string name, object value)
        {
            story.variablesState[name] = value;
        }
        
        // for other scripts to bind EXTERNAL funcs 
        public void BindExternalFunction(string name, Action<string> fn)
        {
            story.BindExternalFunction(name, fn, false);
        }

        public void Tick(double now)
        {
            if (State == RunnerState.NotStarted)
                return;
            this.now = now;

            for (int i = 0; i < MaxIterationsPerTick; i++)
            {
                if (ops.Count > 0)
                {
                    Op head = ops.Peek();
                    if (!head.started)
                    {
                        head.started = true;
                        head.start?.Invoke();
                    }

                    if (head.isDone != null && !head.isDone())
                    {
                        return;
                    }
                    
                    ops.Dequeue();
                }
                else if (story.canContinue)
                {
                    string text = story.Continue();
                    if (!LineParser.IsBlank(text))
                    {
                        EnqueueLine(text);
                    }
                }
                else if (story.currentChoices.Count > 0)
                {
                    if (State != RunnerState.WaitingForChoice && speakers.AllIdle)
                    {
                        PresentChoices();
                    }
                    
                    return;
                }
                else
                {
                    if (State != RunnerState.Ended)
                    {
                        State = RunnerState.Ended;
                        SubtitleBus.RaiseNarrativeEnded();
                    }
                    
                    return;
                }
            }
            
            Debug.LogWarning($"[Subtitles] ink runner hit max iterations in one tick");
        }

        public void SelectChoice(int index)
        {
            string label = pendingChoices[index].label;
            
            story.ChooseChoiceIndex(index);
            SubtitleBus.RaiseChoiceMade(new ChoiceRecord { index = index, label = label, time = now });
            
            turnTracker.MarkBreak();
            pendingChoices.Clear();
            
            State = RunnerState.Running;
        }

        private void EnqueueLine(string text)
        {
            ParsedLine line = LineParser.Parse(text, story.currentTags);
            foreach (string warning in line.warnings) // bad parse 
            {
                Debug.LogWarning($"[Subtitles] {warning} in \"{line.displayText}\""); 
            }

            string speakerId = line.speakerId ?? lastSpeakerId;
            lastSpeakerId = speakerId;
            TurnAssignment turn = turnTracker.Assign(speakerId, line.newTurn);

            if (!speakers.TryGet(speakerId, out SpeakerQueue queue))
            {
                Debug.LogError($"[Subtitles] skipping unknown speaker '{speakerId}': \"{line.displayText}\"");
                return;
            }
            
            SegmentRequest request = new()
            {
                displayText = line.displayText,
                countedText = line.countedText,
                turnId = turn.turnId,
                indexInTurn = turn.indexInTurn,
                rate = queue.Profile.speechRate,
                delaySeconds = line.delaySeconds ?? 0,
                volume = line.volume,
                textSize = line.textSize,
            };
            
            SegmentPlayback playback = null;
            
            ops.Enqueue(new Op
            {
                start = () => playback = queue.Enqueue(request),
                isDone = line.isAsync ? null : () => playback.IsDone,
            });
        }

        private void PresentChoices()
        {
            pendingChoices.Clear();
            List<Choice> choices = story.currentChoices;
            
            for (int i = 0; i < choices.Count; i++)
            {
                pendingChoices.Add(new ChoiceOption { index = i, label = choices[i].text });
            }
            
            State = RunnerState.WaitingForChoice;
            ChoicesPresented?.Invoke(pendingChoices);
        }

        // EXTERNAL funcs in ink file. enqueues an Op and breaks the turn 
        private void BindExternals()
        {
            story.BindExternalFunction("wait_for", (string eventId) => EnqueueWaitFor(eventId), false);
            story.BindExternalFunction("wait_seconds", (float seconds) =>
            {
                double endAt = 0;
                EnqueueAction(() => endAt = now + seconds, () => now >= endAt);
            }, false);
            story.BindExternalFunction("wait_all", () => EnqueueAction(null, () => speakers.AllIdle), false);
            story.BindExternalFunction("event", (string eventId) => EnqueueAction(() => GameEvents.Raise(eventId), null), false);
        }
        
        private void EnqueueWaitFor(string eventId)
        {
            bool raised = false;
            void HandleRaised(string id)
            {
                if (id != eventId)
                {
                    return;
                }
                
                raised = true;
                GameEvents.Raised -= HandleRaised;
            }
            EnqueueAction(() => GameEvents.Raised += HandleRaised, () => raised);
        }

        private void EnqueueAction(Action start, Func<bool> isDone)
        {
            ops.Enqueue(new Op { start = start, isDone = isDone });
            turnTracker.MarkBreak();
        }
    }
    
    public static class GameEvents
    {
        public static event Action<string> Raised;

        public static void Raise(string eventId)
        {
            Debug.Log($"[Subtitles] event {eventId}");
            Raised?.Invoke(eventId);
        }
    }
}
