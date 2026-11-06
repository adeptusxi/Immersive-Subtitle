using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class DockedIndicatorCue : VisualSpeechCue
    {
        [SerializeField] private DockedIndicator template;
        [SerializeField] private RectTransform dockArea;

        private readonly Dictionary<SpeakerProfile, DockedIndicator> indicators = new();
        private readonly HashSet<DockedIndicator> shown = new();

        private const float behindThreshold = 0.1f; // if speaker is almost directly behind, it has no stable direction

        protected override SpeechCueMode Mode => SpeechCueMode.DockedIndicator;

        private void Awake()
        {
            template.gameObject.SetActive(false);
        }

        protected override void UpdateCue()
        {
            shown.Clear();

            foreach (var queue in SpeakerRegistry.Global.All)
            {
                SpeakerProfile speaker = queue.Profile;
                Transform root = SpeakerRegistry.Global.RootOf(speaker);
                if (queue.IsIdle || root == null)
                {
                    continue;
                }

                bool wasOutOfView = indicators.TryGetValue(speaker, out DockedIndicator indicator) && indicator.gameObject.activeSelf;
                if (!IsOutOfView(root, wasOutOfView))
                {
                    continue;
                }

                if (indicator == null)
                {
                    indicator = Instantiate(template, dockArea);
                    indicator.Bind(speaker);
                    indicators[speaker] = indicator;
                }

                if (!wasOutOfView)
                {
                    indicator.gameObject.SetActive(true);
                }

                shown.Add(indicator);
                Place(indicator, root);
            }

            foreach (var entry in indicators)
            {
                if (entry.Value.gameObject.activeSelf && !shown.Contains(entry.Value))
                {
                    entry.Value.gameObject.SetActive(false);
                }
            }
        }

        protected override void HideCue()
        {
            foreach (DockedIndicator indicator in indicators.Values)
            {
                indicator.gameObject.SetActive(false);
            }
        }

        private void Place(DockedIndicator indicator, Transform root)
        {
            Vector3 local = Player.Instance.Head.InverseTransformPoint(root.position);
            Vector2 flat = new Vector2(local.x, local.y);
            if (flat.magnitude >= behindThreshold * local.magnitude)
            {
                indicator.SetDirection(flat.normalized);
            }

            Rect rect = dockArea.rect;
            Vector2 halfSize = rect.size * 0.5f;
            Vector2 direction = indicator.Direction;
            float toEdge = Mathf.Min(halfSize.x / Mathf.Abs(direction.x), halfSize.y / Mathf.Abs(direction.y));

            indicator.transform.localPosition = rect.center + direction * toEdge;
            indicator.PointArrowAt(Player.Instance.Head, root.position);
        }
    }
}
