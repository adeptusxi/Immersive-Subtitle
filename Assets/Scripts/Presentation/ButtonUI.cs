using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TMP_Text label;

        [Header("Scale")]
        [SerializeField] private bool scaleEnabled = true;
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField, Min(0.01f)] private float transitionSpeed = 15f;

        private Button button;
        private Vector3 baseScale;
        private bool hovered;
        private bool pressed;

        public event Action Clicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(HandleClick);

            hovered = false;
            pressed = false;
            transform.localScale = baseScale;
        }

        private void Update()
        {
            if (!scaleEnabled)
            {
                return;
            }

            float t = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, TargetScale(), t);
        }

        public void SetLabel(string text, TMP_FontAsset font)
        {
            label.text = text;
            label.font = font;
        }

        #region Pointer events

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

        #endregion

        private void HandleClick()
        {
            Clicked?.Invoke();
        }

        private Vector3 TargetScale()
        {
            if (!button.interactable)
            {
                return baseScale;
            }

            if (pressed)
            {
                return baseScale * pressScale;
            }

            return hovered ? baseScale * hoverScale : baseScale;
        }
    }
}
