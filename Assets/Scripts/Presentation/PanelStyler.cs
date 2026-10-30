using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    public static class SubtitleStyler
    {
        // name and speaker may be null, e.g. for grouped presentation 
        public static void Apply(TextStylePreferences style, TMP_Text body, TMP_Text name, Image background, SpeakerProfile speaker)
        {
            TMP_FontAsset font = style.font != null ? style.font : TMP_Settings.defaultFontAsset;
            ApplyText(style, font, body, style.textColor);

            if (name != null)
            {
                name.gameObject.SetActive(style.showSpeakerName);
                Color nameColor = style.colorNameBySpeaker && speaker != null ? speaker.color : style.textColor;
                ApplyText(style, font, name, nameColor);
            }

            Color backgroundColor = style.backgroundColor;
            backgroundColor.a = style.backgroundOpacity;
            background.color = backgroundColor;
        }
        
        public static void SetEmHeight(Transform root, float worldEmHeight, float authoredFontSize)
        {
            float scale = worldEmHeight / authoredFontSize;
            Vector3 parentScale = root.parent != null ? root.parent.lossyScale : Vector3.one;
            root.localScale = new Vector3(scale / parentScale.x, scale / parentScale.y, scale / parentScale.z);
        }

        private static void ApplyText(TextStylePreferences style, TMP_FontAsset font, TMP_Text text, Color color)
        {
            text.font = font;
            text.color = color;

            switch (style.alignment)
            {
                case SubtitleAlignment.Left:
                    text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                    break;
                case SubtitleAlignment.Right:
                    text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                    break;
                default:
                    text.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    break;
            }

            Material material = text.fontMaterial;
            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, style.outlineEnabled ? style.outlineWidth : 0f);
            material.SetColor(ShaderUtilities.ID_OutlineColor, style.outlineColor);
            text.UpdateMeshPadding();
        }
    }
}
