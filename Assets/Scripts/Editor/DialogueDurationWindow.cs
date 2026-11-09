using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ImmersiveSubtitles.Core;
using Ink.Runtime;
using Ink.UnityIntegration;
using UnityEditor;
using UnityEngine;

namespace ImmersiveSubtitles.EditorTools
{
    public sealed class DialogueDurationWindow : EditorWindow
    {
        [SerializeField] private InkFile ink;
        [SerializeField] private TimingProfile timing;
        [SerializeField] private float secondsPerChoice = 0f;

        private string result;
        private string warnings;
        private Vector2 scroll;
        private GUIStyle resultStyle;

        private const string DefaultTimingProfilePath = "Assets/Content/Settings/TimingProfile.asset";

        [MenuItem("Tools/Immersive Subtitles/Dialogue Duration")]
        private static void Open()
        {
            GetWindow<DialogueDurationWindow>("Dialogue Duration");
        }

        private void OnEnable()
        {
            if (timing == null)
            {
                timing = AssetDatabase.LoadAssetAtPath<TimingProfile>(DefaultTimingProfilePath);
            }
        }

        private void OnGUI()
        {
            resultStyle ??= new GUIStyle(EditorStyles.helpBox) { fontSize = 13, wordWrap = true, padding = new RectOffset(8, 8, 8, 8) };

            EditorGUILayout.LabelField("Drag in an Ink file and a timing profile, then press Calculate.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            ink = (InkFile)EditorGUILayout.ObjectField("Ink File", ink, typeof(InkFile), false);
            timing = (TimingProfile)EditorGUILayout.ObjectField("Timing Profile", timing, typeof(TimingProfile), false);
            secondsPerChoice = Mathf.Max(0f, EditorGUILayout.FloatField(new GUIContent("Seconds Per Choice", "Time the player spends reading and picking each choice."), secondsPerChoice));
            if (EditorGUI.EndChangeCheck())
            {
                result = null;
                warnings = null;
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(ink == null || timing == null))
            {
                if (GUILayout.Button("Calculate", GUILayout.Height(30)))
                {
                    Calculate();
                }
            }

            EditorGUILayout.Space();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (result != null)
            {
                GUILayout.Label(result, resultStyle);
            }
            if (warnings != null)
            {
                EditorGUILayout.HelpBox(warnings, MessageType.Warning);
            }
            EditorGUILayout.EndScrollView();
        }

        private void Calculate()
        {
            if (!ink.isCompiled)
            {
                result = null;
                warnings = "This Ink file has syntax issues. Fix its errors and try again.";
                return;
            }

            var simulation = new Simulation(ink.storyJson, timing, secondsPerChoice, LoadSpeakers());
            simulation.Run();
            result = simulation.Summary();
            warnings = simulation.Warnings.Count > 0 ? string.Join("\n\n", simulation.Warnings) : null;
        }

        private static Dictionary<string, SpeakerProfile> LoadSpeakers()
        {
            var speakers = new Dictionary<string, SpeakerProfile>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets("t:SpeakerProfile"))
            {
                var profile = AssetDatabase.LoadAssetAtPath<SpeakerProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if (!string.IsNullOrEmpty(profile.id))
                {
                    speakers.TryAdd(profile.id, profile);
                }
            }
            return speakers;
        }

        private class PathState
        {
            public double time;
            public Dictionary<string, double> speakerFreeAt = new(StringComparer.OrdinalIgnoreCase);
            public string lastSpeakerId;
            public HashSet<string> seenLines = new();
            public HashSet<string> seenChoicePoints = new();
            public bool choicePending; 
            public int lineCount;

            public double AllIdleAt => speakerFreeAt.Values.Aggregate(time, Math.Max);

            public PathState Clone()
            {
                return new PathState
                {
                    time = time,
                    speakerFreeAt = new Dictionary<string, double>(speakerFreeAt, StringComparer.OrdinalIgnoreCase),
                    lastSpeakerId = lastSpeakerId,
                    seenLines = new HashSet<string>(seenLines),
                    seenChoicePoints = new HashSet<string>(seenChoicePoints),
                    choicePending = choicePending,
                    lineCount = lineCount,
                };
            }
        }

        private class Simulation
        {
            private const string DefaultSpeakerId = "alien1"; 
            private const int MaxPaths = 2000;
            private const int MaxLinesPerPath = 10000;

            private readonly Story story;
            private readonly TimingProfile timing;
            private readonly double secondsPerChoice;
            private readonly Dictionary<string, SpeakerProfile> speakers;
            private readonly List<string> waitedFor = new();
            private readonly HashSet<string> cappedLines = new();
            private PathState path;
            private int pathCount;
            private double shortest = double.MaxValue;
            private double longest;
            private bool looped;
            private bool hitPathCap;

            public List<string> Warnings { get; } = new();

            public Simulation(string storyJson, TimingProfile timing, double secondsPerChoice, Dictionary<string, SpeakerProfile> speakers)
            {
                this.timing = timing;
                this.secondsPerChoice = secondsPerChoice;
                this.speakers = speakers;

                story = new Story(storyJson);
                story.allowExternalFunctionFallbacks = true;   // externals this tool doesn't know use their ink fallback
                story.onError += (message, type) => AddUnique(Warnings, $"Ink {type}: {message}");

                story.BindExternalFunction("wait_for", (string eventId) => { AddUnique(waitedFor, eventId); }, false);
                story.BindExternalFunction("wait_seconds", (float seconds) => { path.time += seconds; }, false);
                story.BindExternalFunction("wait_all", () => { path.time = path.AllIdleAt; }, false);
                story.BindExternalFunction("event", (string eventId) => { }, false);
            }

            public void Run()
            {
                path = new PathState { lastSpeakerId = DefaultSpeakerId };
                Explore();

                if (hitPathCap)
                {
                    Warnings.Add($"This story has a lot of choice branches. Only the first {MaxPaths} were checked.");
                }
                
                if (cappedLines.Count > 0)
                {
                    Warnings.Add($"{cappedLines.Count} lines are longer than allowed ({timing.maxSegmentSeconds} s) and will be cut short in the game. Consider splitting them:\n" + string.Join("\n", cappedLines.Select(l => $"  \"{l}\"")));
                }
            }

            public string Summary()
            {
                if (pathCount == 0)
                {
                    return "Couldn't calculate a duration for this dialogue.";
                }

                string shortText = Describe(shortest);
                string longText = Describe(longest);
                var text = new StringBuilder("This dialogue will take about ");
                text.Append(shortText == longText ? shortText : $"{shortText} to {longText}, depending on the player's choices");
                if (waitedFor.Count > 0)
                {
                    text.Append(", excluding the time required for:");
                    foreach (string eventId in waitedFor)
                    {
                        text.Append($"\n- {eventId}");
                    }
                }
                else
                {
                    text.Append('.');
                }

                if (looped)
                {
                    text.Append("\n\nThis story loops back to earlier lines. Only one time through is counted.");
                }
                
                text.Append($"\n\nChecked {pathCount} possible path(s) through the choices.");
                
                return text.ToString();
            }

            // follows story branches from current state 
            private void Explore()
            {
                while (true)
                {
                    if (story.canContinue)
                    {
                        if (++path.lineCount > MaxLinesPerPath)
                        {
                            AddUnique(Warnings, "This story ran for too long - the simulation cut off, so the result might be an underestimate.");
                            FinishPath();
                            return;
                        }

                        double timeBefore = path.time;
                        string text = story.Continue();
                        if (IsRepeat(story.state.currentPathString))
                        {
                            // undo waits called just before the repeated line, they were counted the first time
                            path.time = timeBefore;
                            looped = true;
                            FinishPath();
                            return;
                        }

                        if (!LineParser.IsBlank(text))
                        {
                            PlayLine(text);
                        }
                    }
                    else if (story.currentChoices.Count > 0)
                    {
                        if (!path.seenChoicePoints.Add(string.Join("|", story.currentChoices.Select(c => c.sourcePath))))
                        {
                            looped = true;
                            FinishPath();
                            return;
                        }

                        CommitPendingChoice();
                        path.time = path.AllIdleAt; 
                        path.choicePending = true;

                        string savedStory = story.state.ToJson();
                        PathState savedPath = path;
                        int optionCount = story.currentChoices.Count;
                        for (int i = 0; i < optionCount; i++)
                        {
                            if (pathCount >= MaxPaths)
                            {
                                hitPathCap = true;
                                return;
                            }
                            story.state.LoadJson(savedStory);
                            path = savedPath.Clone();
                            story.ChooseChoiceIndex(i);
                            Explore();
                        }
                        return;
                    }
                    else
                    {
                        CommitPendingChoice();
                        FinishPath();
                        return;
                    }
                }
            }

            private bool IsRepeat(string pathString)
            {
                if (pathString == null)
                {
                    return false;
                }
                
                return !path.seenLines.Add(pathString + story.state.callStack.callStackTrace);
            }

            // same scheduling as SpeakerQueue 
            private void PlayLine(string text)
            {
                ParsedLine line = LineParser.Parse(text, story.currentTags);
                foreach (string warning in line.warnings)
                {
                    AddUnique(Warnings, $"{warning} in \"{line.displayText}\"");
                }

                string speakerId = line.speakerId ?? path.lastSpeakerId;
                path.lastSpeakerId = speakerId;
                CommitPendingChoice();

                if (!speakers.TryGetValue(speakerId, out SpeakerProfile profile))
                {
                    AddUnique(Warnings, $"Unknown speaker \"{speakerId}\": their lines are skipped in game, so they aren't counted.");
                    return;
                }

                SegmentTiming segment = TimingCalculator.Compute(line.countedText, profile.speechRate, timing);
                if (segment.hitMax)
                {
                    cappedLines.Add(line.displayText);
                }

                double start = path.speakerFreeAt.TryGetValue(speakerId, out double freeAt) ? Math.Max(path.time, freeAt) : path.time;
                double end = start + (line.delaySeconds ?? 0) + segment.duration + segment.gap;
                path.speakerFreeAt[speakerId] = end;
                if (!line.isAsync)
                {
                    path.time = end;
                }
            }

            private void CommitPendingChoice()
            {
                if (path.choicePending)
                {
                    path.time += secondsPerChoice;
                    path.choicePending = false;
                }
            }

            private void FinishPath()
            {
                double total = path.AllIdleAt;
                pathCount++;
                shortest = Math.Min(shortest, total);
                longest = Math.Max(longest, total);
            }

            private static string Describe(double seconds)
            {
                if (seconds < 60)
                {
                    return $"{Math.Round(seconds).ToString(CultureInfo.InvariantCulture)} seconds";
                }
                return $"{(seconds / 60).ToString("0.0", CultureInfo.InvariantCulture)} minutes";
            }

            private static void AddUnique(List<string> list, string item)
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
        }
    }
}
