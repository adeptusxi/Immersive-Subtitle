using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    // one UI object per speaker, docked to edge of player's fov in the speaker's direction 
    public abstract class DockedCue<TItem> : VisualSpeechCue where TItem : DockedItem
    {
        [SerializeField] private TItem template;
        [SerializeField] private RectTransform dockArea;
        [SerializeField] private float spacing;
        [SerializeField, Tooltip("extra distance in from the dock area's edge, in dock area units")]
        private float edgeInset;

        [Header("Scale In/Out")]
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField, Tooltip("normalized: time 0..1 → scale 0..1")]
        private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private struct Slot
        {
            public TItem item;
            public Transform root;
            public Vector2 halfSize; // dock rect inset by the item's HalfExtents
            public float t; // position along the perimeter, 0->1 clockwise from top center
            public float halfSpan; // half the perimeter fraction the item needs, including spacing
        }

        private readonly Dictionary<SpeakerProfile, TItem> items = new();
        private readonly Dictionary<TItem, float> scaleProgress = new(); // 0 hidden → 1 fully shown
        private HashSet<TItem> wanted = new();
        private HashSet<TItem> wasWanted = new();
        private readonly List<Slot> slots = new();

        private const int spreadPasses = 10;

        private const float behindThreshold = 0.1f; // if speaker is almost directly behind, it has no stable direction

        protected IReadOnlyDictionary<SpeakerProfile, TItem> Items => items;

        protected virtual void Awake()
        {
            template.gameObject.SetActive(false);
        }

        protected abstract bool IsSpeaking(SpeakerQueue queue);

        protected virtual void OnShown(TItem item, SpeakerProfile speaker) { }

        protected virtual void OnHidden(TItem item, SpeakerProfile speaker) { }

        protected virtual void UpdateItem(TItem item, SpeakerProfile speaker) { }

        protected override void UpdateCue()
        {
            (wanted, wasWanted) = (wasWanted, wanted);
            wanted.Clear();
            slots.Clear();

            foreach (var queue in SpeakerRegistry.Global.All)
            {
                SpeakerProfile speaker = queue.Profile;
                Transform root = SpeakerRegistry.Global.RootOf(speaker);
                if (root == null || !IsSpeaking(queue))
                {
                    continue;
                }

                bool wasOutOfView = items.TryGetValue(speaker, out TItem item) && wasWanted.Contains(item);
                if (!IsOutOfView(root, wasOutOfView))
                {
                    continue;
                }

                if (item == null)
                {
                    item = Instantiate(template, dockArea);
                    item.Bind(speaker);
                    items[speaker] = item;
                }

                if (!item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(true);
                    scaleProgress[item] = 0f;
                    OnShown(item, speaker);
                }

                wanted.Add(item);
                UpdateItem(item, speaker);
                slots.Add(IdealSlot(item, root));
            }

            Spread();
            foreach (Slot slot in slots)
            {
                slot.item.transform.localPosition = dockArea.rect.center + FromPerimeter(slot.t, slot.halfSize);
                slot.item.OnPlaced(Player.Instance.Head, slot.root.position);
            }

            float step = scaleDuration > 0f ? Time.unscaledDeltaTime / scaleDuration : 1f;
            foreach (var entry in items)
            {
                TItem item = entry.Value;
                if (!item.gameObject.activeSelf)
                {
                    continue;
                }

                bool isWanted = wanted.Contains(item);
                float progress = Mathf.Clamp01(scaleProgress[item] + (isWanted ? step : -step));
                scaleProgress[item] = progress;
                if (!isWanted && progress <= 0f)
                {
                    Hide(item, entry.Key);
                }
                else
                {
                    item.SetScaleFactor(scaleCurve.Evaluate(progress));
                }
            }
        }

        protected override void HideCue()
        {
            foreach (var entry in items)
            {
                if (entry.Value.gameObject.activeSelf)
                {
                    Hide(entry.Value, entry.Key);
                }
            }

            wanted.Clear();
            wasWanted.Clear();
        }

        private void Hide(TItem item, SpeakerProfile speaker)
        {
            item.gameObject.SetActive(false);
            OnHidden(item, speaker);
        }

        #region Placement

        // the slot before accounting for overlapping 
        private Slot IdealSlot(TItem item, Transform root)
        {
            Vector3 local = Player.Instance.Head.InverseTransformPoint(root.position);
            Vector2 flat = new Vector2(local.x, local.y);
            if (flat.magnitude >= behindThreshold * local.magnitude)
            {
                item.SetDirection(flat.normalized);
            }

            Vector2 halfSize = Vector2.Max(dockArea.rect.size * 0.5f - item.HalfExtents - Vector2.one * edgeInset, Vector2.zero);
            Vector2 direction = item.Direction;
            float toEdge = Mathf.Min(halfSize.x / Mathf.Abs(direction.x), halfSize.y / Mathf.Abs(direction.y));
            Vector2 point = direction * toEdge;

            float perimeter = 4f * (halfSize.x + halfSize.y);
            float extent = IsOnSide(point, halfSize) ? item.Size.y : item.Size.x;
            return new Slot
            {
                item = item,
                root = root,
                halfSize = halfSize,
                t = ToPerimeter(point, halfSize),
                halfSpan = perimeter > 0f ? (extent + spacing) * 0.5f / perimeter : 0f,
            };
        }

        // push neighbors apart 
        private void Spread()
        {
            int count = slots.Count;
            if (count < 2)
            {
                return;
            }

            slots.Sort((a, b) => a.t.CompareTo(b.t));

            float total = 0f;
            foreach (Slot slot in slots)
            {
                total += 2f * slot.halfSpan;
            }

            if (total >= 1f)
            {
                for (int i = 0; i < count; i++)
                {
                    Slot slot = slots[i];
                    slot.t = Mathf.Repeat(slots[0].t + (float)i / count, 1f);
                    slots[i] = slot;
                }

                return;
            }

            for (int pass = 0; pass < spreadPasses; pass++)
            {
                for (int i = 0; i < count; i++)
                {
                    int next = (i + 1) % count;
                    Slot a = slots[i];
                    Slot b = slots[next];
                    float gap = b.t - a.t + (next == 0 ? 1f : 0f);
                    float needed = a.halfSpan + b.halfSpan;
                    if (gap >= needed)
                    {
                        continue;
                    }

                    float push = (needed - gap) * 0.5f;
                    a.t -= push;
                    b.t += push;
                    slots[i] = a;
                    slots[next] = b;
                }
            }

            for (int i = 0; i < count; i++)
            {
                Slot slot = slots[i];
                slot.t = Mathf.Repeat(slot.t, 1f);
                slots[i] = slot;
            }
        }

        private static bool IsOnSide(Vector2 point, Vector2 halfSize)
        {
            return Mathf.Abs(point.x) * halfSize.y >= Mathf.Abs(point.y) * halfSize.x;
        }

        // convert point to 0->1 value along perimeter, clockwise from top centre
        private static float ToPerimeter(Vector2 point, Vector2 halfSize)
        {
            float hx = halfSize.x;
            float hy = halfSize.y;
            float perimeter = 4f * (hx + hy);
            if (perimeter <= 0f)
            {
                return 0f;
            }

            float distance;
            if (IsOnSide(point, halfSize))
            {
                distance = point.x > 0f ? hx + (hy - point.y) : 3f * hx + 2f * hy + (point.y + hy);
            }
            else if (point.y < 0f)
            {
                distance = hx + 2f * hy + (hx - point.x);
            }
            else
            {
                distance = point.x >= 0f ? point.x : perimeter + point.x;
            }

            return Mathf.Repeat(distance / perimeter, 1f);
        }

        private static Vector2 FromPerimeter(float t, Vector2 halfSize)
        {
            float hx = halfSize.x;
            float hy = halfSize.y;
            float distance = t * 4f * (hx + hy);

            if (distance < hx)
            {
                return new Vector2(distance, hy);
            }

            distance -= hx;
            if (distance < 2f * hy)
            {
                return new Vector2(hx, hy - distance);
            }

            distance -= 2f * hy;
            if (distance < 2f * hx)
            {
                return new Vector2(hx - distance, -hy);
            }

            distance -= 2f * hx;
            if (distance < 2f * hy)
            {
                return new Vector2(-hx, -hy + distance);
            }

            distance -= 2f * hy;
            return new Vector2(-hx + distance, hy);
        }

        #endregion
    }
}
