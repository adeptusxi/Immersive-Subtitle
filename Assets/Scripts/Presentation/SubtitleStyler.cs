using ImmersiveSubtitles.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    public static class SubtitleStyler
    {
        // name, background and speaker can be null 
        public static void Apply(TextStylePreferences style, TMP_Text body, TMP_Text name, Image background, SpeakerProfile speaker)
        {
            TMP_FontAsset font = style.font != null ? style.font : TMP_Settings.defaultFontAsset;
            body.font = font;
            body.color = style.colorTextBySpeaker && speaker != null ? speaker.color : style.textColor;

            if (name != null)
            {
                name.font = font;
                name.color = speaker != null ? speaker.color : style.textColor;
            }

            if (background != null) 
            {
                Color backgroundColor = style.backgroundColor;
                backgroundColor.a = style.backgroundOpacity;
                background.color = backgroundColor;
            }
        }
        
        public static void ClearUnusedSubMeshes(TMP_Text text)
        {
            TMP_TextInfo info = text.textInfo;
            for (int i = info.materialCount; i < info.meshInfo.Length; i++)
            {
                info.meshInfo[i].Clear(true);
            }
        }

        public static void SetEmHeight(Transform root, float worldEmHeight, float authoredFontSize)
        {
            float scale = worldEmHeight / authoredFontSize;
            Vector3 parentScale = root.parent != null ? root.parent.lossyScale : Vector3.one;
            root.localScale = new Vector3(scale / parentScale.x, scale / parentScale.y, scale / parentScale.z);
        }
    }
}
