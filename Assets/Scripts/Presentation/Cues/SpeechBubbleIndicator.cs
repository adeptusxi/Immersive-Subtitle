using ImmersiveSubtitles.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class SpeechBubbleIndicator : DockedWordIndicator
    {
        [SerializeField] private RectTransform bubble;
        [SerializeField] private Graphic[] outlines;
        [SerializeField] private float tailInset;
        [SerializeField] private RectTransform tailOutlinePivot;
        
        protected override RectTransform ArrowOrigin => bubble;

        protected override void BindIcon(SpeakerProfile speaker)
        {
            base.BindIcon(speaker);
            foreach (Graphic outline in outlines)
            {
                if (outline != null)
                {
                    outline.color = speaker.color;
                }
            }
        }

        protected override Vector2 ArrowOffset(Vector2 direction)
        {
            Vector3 scale = bubble.localScale;
            float a = Mathf.Max(0.001f, bubble.rect.width * 0.5f * Mathf.Abs(scale.x) - tailInset);
            float b = Mathf.Max(0.001f, bubble.rect.height * 0.5f * Mathf.Abs(scale.y) - tailInset);
            float x = direction.x / a;
            float y = direction.y / b;
            return direction / Mathf.Sqrt(x * x + y * y);
        }

        public override void OnPlaced(Transform head, Vector3 target)
        {
            base.OnPlaced(head, target);

            Vector3 scale = ArrowPivot.localScale;
            ArrowPivot.localScale = scale;

            if (tailOutlinePivot != null)
            {
                tailOutlinePivot.localPosition = ArrowPivot.localPosition;
                tailOutlinePivot.localRotation = ArrowPivot.localRotation;
                tailOutlinePivot.localScale = ArrowPivot.localScale;
            }
        }
    }
}
