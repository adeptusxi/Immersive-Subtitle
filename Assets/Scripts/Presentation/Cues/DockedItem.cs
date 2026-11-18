using ImmersiveSubtitles.Core;
using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    // per-speaker UI object docked to edge of player's vision 
    public abstract class DockedItem : MonoBehaviour
    {
        private Vector3 authoredScale;
        private float sizeFactor = 1f;

        public Vector2 Direction { get; private set; } = Vector2.right;
        
        public virtual Vector2 HalfExtents => Vector2.zero;

        // in dock area units. to prevent items form overlapping 
        public virtual Vector2 Size
        {
            get
            {
                Vector2 size = ((RectTransform)transform).rect.size;
                Vector3 scale = BaseScale * sizeFactor;
                return new Vector2(size.x * scale.x, size.y * scale.y);
            }
        }
        
        protected virtual Vector3 BaseScale => authoredScale; // local scale 

        protected virtual void Awake()
        {
            authoredScale = transform.localScale;
        }

        public virtual void Bind(SpeakerProfile speaker) { }

        public void SetSizeFactor(float factor)
        {
            sizeFactor = factor;
        }

        public void SetScaleFactor(float factor)
        {
            transform.localScale = BaseScale * sizeFactor * factor;
        }

        public void SetDirection(Vector2 direction)
        {
            Direction = direction;
        }

        // called after item is placed
        public virtual void OnPlaced(Transform head, Vector3 target) { }
    }
}
