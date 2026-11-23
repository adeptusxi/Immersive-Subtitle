using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    // speech bubble sizing, layout, styling 
    [ExecuteAlways, DefaultExecutionOrder(190), RequireComponent(typeof(RectTransform))]
    public sealed class SpeechBubble : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private TMP_Text body;
        [SerializeField, Min(1f)] private float maxTextWidth = 1600f;
        [SerializeField] private Vector2 padding = new(40f, 20f);
        [SerializeField, Min(1f)] private float ellipseFit = 1.41421356f; // sqrt(2) puts corners exactly on the oval
        [SerializeField] private Vector2 minSize;
        
        [Header("Style")]
        [SerializeField] private SingleSpeakerPresenter presenter;
        [SerializeField] private SpeechBubbleGraphic graphic;
        [SerializeField] private Image nameBackground;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private bool colorBySpeaker;
        [SerializeField, Tooltip("name card color when colorBySpeaker = true")] private Color tintedNameTextColor = Color.white; 
        
        private DrivenRectTransformTracker tracker;
        private string lastText;
        private float lastFontSize;
        private TMP_FontAsset lastFont;
        private bool layoutDirty = true;
        private bool nameBackgroundCached;
        private Color authoredNameBackgroundColor;

        #region Unity

        private void OnEnable()
        {
            tracker.Add(this, (RectTransform)transform, DrivenTransformProperties.SizeDelta);
            tracker.Add(this, body.rectTransform, DrivenTransformProperties.SizeDelta);
            layoutDirty = true;

            if (Application.isPlaying)
            {
                presenter.StyleApplied += ApplyStyle;
                ApplyStyle(presenter.CurrentTextStyle);
            }
        }

        private void OnDisable()
        {
            tracker.Clear();
            if (Application.isPlaying)
            {
                presenter.StyleApplied -= ApplyStyle;
            }
        }

        private void OnValidate()
        {
            layoutDirty = true;
        }

        private void LateUpdate()
        {
            if (!layoutDirty && body.text == lastText && body.fontSize == lastFontSize && body.font == lastFont)
            {
                return;
            }

            lastText = body.text;
            lastFontSize = body.fontSize;
            lastFont = body.font;
            layoutDirty = false;
            
            Vector2 preferred = body.GetPreferredValues(body.text, maxTextWidth, Mathf.Infinity);
            Vector2 textSize = new(Mathf.Min(preferred.x, maxTextWidth) + 1f, preferred.y);
            body.rectTransform.sizeDelta = textSize;
            ((RectTransform)transform).sizeDelta = Vector2.Max(minSize, (textSize + 2f * padding) * ellipseFit);
        }

        #endregion
        
        private void ApplyStyle(TextStylePreferences style)
        {
            SpeakerProfile speaker = presenter.Speaker;
            if (!nameBackgroundCached)
            {
                authoredNameBackgroundColor = nameBackground.color;
                nameBackgroundCached = true;
            }

            graphic.Opacity = style.backgroundOpacity;
            graphic.OutlineOverride = colorBySpeaker ? (Color?)speaker.color : null;

            Color plate = colorBySpeaker ? speaker.color : authoredNameBackgroundColor;
            plate.a = authoredNameBackgroundColor.a * style.backgroundOpacity;
            nameBackground.color = plate;

            if (colorBySpeaker)
            {
                nameText.color = tintedNameTextColor;
            }
        }
    }
}
