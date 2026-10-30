using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    public sealed class Player : MonoBehaviour
    {
        [SerializeField] private Transform centerEyeAnchor;

        public static Player Instance { get; private set; }
        public Transform Head => centerEyeAnchor;

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
