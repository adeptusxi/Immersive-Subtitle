using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    // docked arrow pointing to a non speaker object 
    [DefaultExecutionOrder(200)]
    public sealed class DockedTargetCue : MonoBehaviour
    {
        [SerializeField] private DockedIndicator template;
        [SerializeField] private RectTransform dockArea;
        [SerializeField] private Transform[] targets = { };
        [SerializeField] private float edgeInset;
        [SerializeField] private float fovMarginDegrees = 3f;

        [Header("Entrance / Exit")]
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Size")]
        [SerializeField] private bool sizeByAngle;
        [SerializeField, Tooltip("multiplier on authored local scale")] private float maxAngleScale = 1f;
        [SerializeField, Tooltip("multiplier on authored local scale")] private float minAngleScale = 0.5f;
        [SerializeField] private float maxScaleAngle = 45f;

        private const float behindThreshold = 0.1f; 

        private readonly Dictionary<Transform, DockedIndicator> items = new();
        private readonly Dictionary<DockedIndicator, float> progress = new(); // 0 hidden, 1 fully shown
        private readonly HashSet<Transform> wasOutOfView = new();
        private readonly HashSet<Transform> wasWanted = new();

        private void Awake()
        {
            template.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            foreach (DockedIndicator item in items.Values)
            {
                item.gameObject.SetActive(false);
            }

            wasOutOfView.Clear();
            wasWanted.Clear();
        }

        private void LateUpdate()
        {
            if (Player.Instance == null)
            {
                return;
            }

            float step = scaleDuration > 0f ? Time.unscaledDeltaTime / scaleDuration : 1f;

            foreach (Transform target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                bool wanted = false;
                if (target.gameObject.activeInHierarchy)
                {
                    float margin = wasOutOfView.Contains(target) ? -fovMarginDegrees : 0f;
                    wanted = !Player.Instance.IsInFOV(target.position, margin);
                }

                if (wanted)
                {
                    wasOutOfView.Add(target);
                }
                else
                {
                    wasOutOfView.Remove(target);
                }

                if (wanted != wasWanted.Contains(target))
                {
                    if (wanted)
                    {
                        wasWanted.Add(target);
                    }
                    else
                    {
                        wasWanted.Remove(target);
                    }
                }

                items.TryGetValue(target, out DockedIndicator item);
                if (wanted)
                {
                    if (item == null)
                    {
                        item = Instantiate(template, dockArea);
                        item.BindTarget();
                        items[target] = item;
                    }

                    if (!item.gameObject.activeSelf)
                    {
                        item.gameObject.SetActive(true);
                        progress[item] = 0f;
                    }

                    Place(item, target);
                }

                if (item == null || !item.gameObject.activeSelf)
                {
                    continue;
                }

                float p = Mathf.Clamp01(progress[item] + (wanted ? step : -step));
                progress[item] = p;
                if (!wanted && p <= 0f)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
                    item.SetScaleFactor(scaleCurve.Evaluate(p));
                }
            }
        }

        private void Place(DockedIndicator item, Transform target)
        {
            Transform head = Player.Instance.Head;
            Vector3 local = head.InverseTransformPoint(target.position);
            item.SetSizeFactor(sizeByAngle ? AngleSizeFactor(local) : 1f);

            Vector2 flat = new Vector2(local.x, local.y);
            if (flat.magnitude >= behindThreshold * local.magnitude)
            {
                item.SetDirection(flat.normalized);
            }

            Vector2 halfSize = Vector2.Max(dockArea.rect.size * 0.5f - item.HalfExtents - Vector2.one * edgeInset, Vector2.zero);
            Vector2 direction = item.Direction;
            float toEdge = Mathf.Min(halfSize.x / Mathf.Abs(direction.x), halfSize.y / Mathf.Abs(direction.y));

            item.transform.localPosition = dockArea.rect.center + direction * toEdge;
            item.OnPlaced(head, target.position);
        }

        private float AngleSizeFactor(Vector3 headLocal)
        {
            float angle = Vector3.Angle(Vector3.forward, headLocal);
            return Mathf.Lerp(maxAngleScale, minAngleScale, Mathf.InverseLerp(maxScaleAngle, 180f, angle));
        }
    }
}
