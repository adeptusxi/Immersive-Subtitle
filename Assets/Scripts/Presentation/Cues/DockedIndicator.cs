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

        public override void Bind(SpeakerProfile speaker)
        {
            icon.sprite = speaker.icon;
            arrowRadius = Vector2.Distance(arrowPivot.localPosition, icon.rectTransform.localPosition);
        }

        public override void OnPlaced(Transform head, Vector3 target)
        {
            Vector3 origin = head.InverseTransformPoint(icon.rectTransform.position);
            Vector3 speaker = head.InverseTransformPoint(target);

            Vector3 toSpeaker = new Vector3(speaker.x * origin.z - origin.x * speaker.z, speaker.y * origin.z - origin.y * speaker.z, 0f);
            Vector3 local = arrowPivot.parent.InverseTransformDirection(head.TransformDirection(toSpeaker));
            Vector2 direction = new Vector2(local.x, local.y).normalized;
            ArrowDirection = direction;

            arrowPivot.localPosition = (Vector2)icon.rectTransform.localPosition + direction * arrowRadius;
            arrowPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }
    }
}
