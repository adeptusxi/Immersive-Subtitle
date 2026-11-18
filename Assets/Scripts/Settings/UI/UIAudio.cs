using UnityEngine;

namespace ImmersiveSubtitles.Presentation
{
    public sealed class UIAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource click;
        [SerializeField] private AudioSource hover;

        private void Awake()
        {
            if (click)
            {
                click.playOnAwake = false;
                click.spatialize = false;
                click.spatialBlend = 0f;
            }
            
            if (hover)
            {
                hover.playOnAwake = false;
                hover.spatialize = false;
                hover.spatialBlend = 0f;
            }
        }

        private void OnEnable()
        {
            ButtonUI.AnyClicked += HandleClick;
            ButtonUI.AnyHovered +=  HandleHover;
        }

        private void OnDisable()
        {
            ButtonUI.AnyClicked -= HandleClick;
            ButtonUI.AnyHovered -= HandleHover;
        }

        private void HandleClick()
        {
            if (click) click.PlayOneShot(click.clip);
        }

        private void HandleHover()
        {
            if (hover) hover.PlayOneShot(hover.clip);
        }
    }
}
