using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    public sealed class Player : MonoBehaviour
    {
        [SerializeField] private Transform centerEyeAnchor;
        [SerializeField] private float playerFOV = 90f;

        public static Player Instance { get; private set; }
        public Transform Head => centerEyeAnchor;

        public bool IsInFOV(Vector3 worldPosition)
        {
            Vector3 forward = Vector3.ProjectOnPlane(Head.forward, Vector3.up);
            Vector3 toTarget = Vector3.ProjectOnPlane(worldPosition - Head.position, Vector3.up);
            return Vector3.Angle(forward, toTarget) <= playerFOV * 0.5f;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
