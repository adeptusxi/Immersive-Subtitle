using ImmersiveSubtitles.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    public class DockedIndicator : DockedItem
    {
        [SerializeField] private Image icon;
        [SerializeField] private RectTransform arrowPivot; // arrow child should point up in scene

        private float arrowRadius;

        // direction the arrow points in the pivot's parent space
        protected Vector2 ArrowDirection { get; private set; }

        protected RectTransform ArrowPivot => arrowPivot;
        protected virtual RectTransform ArrowOrigin => icon != null ? icon.rectTransform : null;
        private Transform Origin => ArrowOrigin != null ? ArrowOrigin : transform;
        private Vector2 OriginLocal => ArrowOrigin != null ? (Vector2)ArrowOrigin.localPosition : Vector2.zero; // origin in the arrow pivot's parent space 
        protected virtual Vector2 ArrowOffset(Vector2 direction) => direction * arrowRadius; // arrow pivot offset from origin for a unit direction in pivot's parent space
        
        public override void Bind(SpeakerProfile speaker)
        {
            BindIcon(speaker);
            CacheArrowRadius();
        }
        
        // for non speaker target 
        public void BindTarget()
        {
            CacheArrowRadius();
        }

        private void CacheArrowRadius()
        {
            arrowRadius = Vector2.Distance(arrowPivot.localPosition, OriginLocal);
        }

        protected virtual void BindIcon(SpeakerProfile speaker)
        {
            if (icon != null)
            {
                icon.sprite = speaker.icon;
            }
        }
        
        public override void OnPlaced(Transform head, Vector3 target)
        {
            Vector3 origin = head.InverseTransformPoint(Origin.position);
            Vector3 speaker = head.InverseTransformPoint(target);

            Vector3 toSpeaker = new Vector3(speaker.x * origin.z - origin.x * speaker.z, speaker.y * origin.z - origin.y * speaker.z, 0f);
            Vector3 local = arrowPivot.parent.InverseTransformDirection(head.TransformDirection(toSpeaker));
            Vector2 direction = new Vector2(local.x, local.y).normalized;
            ArrowDirection = direction;

            arrowPivot.localPosition = OriginLocal + ArrowOffset(direction);
            arrowPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }
    }
}
