using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    public sealed class Player : MonoBehaviour
    {
        [SerializeField] private Transform centerEyeAnchor;
        [SerializeField] private float playerHorizontalFOV = 90f;
        [SerializeField] private float playerVerticalFOV = 75f;

        public static Player Instance { get; private set; }
        public Transform Head => centerEyeAnchor;
        
        public bool IsInFOV(Vector3 worldPosition, float marginDegrees = 0f)
        {
            Vector3 local = Head.InverseTransformPoint(worldPosition);
            float horizontal = Mathf.Atan2(Mathf.Abs(local.x), local.z) * Mathf.Rad2Deg;
            float vertical = Mathf.Atan2(Mathf.Abs(local.y), local.z) * Mathf.Rad2Deg;
            return horizontal <= playerHorizontalFOV * 0.5f + marginDegrees && vertical <= playerVerticalFOV * 0.5f + marginDegrees;
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
