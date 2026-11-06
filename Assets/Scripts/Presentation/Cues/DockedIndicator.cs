using ImmersiveSubtitles.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class DockedIndicator : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private RectTransform arrowPivot; // arrow child should point up in scene

        public Vector2 Direction { get; private set; } = Vector2.right;

        public void Bind(SpeakerProfile speaker)
        {
            icon.sprite = speaker.icon;
        }

        public void SetDirection(Vector2 direction)
        {
            Direction = direction;
        }

        public void PointArrowAt(Transform head, Vector3 target)
        {
            Vector3 arrow = head.InverseTransformPoint(arrowPivot.position);
            Vector3 speaker = head.InverseTransformPoint(target);
            
            Vector3 toSpeaker = new Vector3(speaker.x * arrow.z - arrow.x * speaker.z, speaker.y * arrow.z - arrow.y * speaker.z, 0f);
            Vector3 local = arrowPivot.parent.InverseTransformDirection(head.TransformDirection(toSpeaker));
            arrowPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg - 90f);
        }
    }
}
