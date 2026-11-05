using ImmersiveSubtitles.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class DockedIndicator : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private RectTransform arrowPivot;

        public Vector2 Direction { get; private set; } = Vector2.right;

        public void Bind(SpeakerProfile speaker)
        {
            icon.sprite = speaker.icon;
        }

        public void SetDirection(Vector2 direction)
        {
            Direction = direction;
            arrowPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
    }
}
