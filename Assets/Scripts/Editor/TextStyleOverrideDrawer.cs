using ImmersiveSubtitles.Core;
using UnityEditor;
using UnityEngine;

namespace ImmersiveSubtitles.EditorTools
{
    [CustomPropertyDrawer(typeof(TextStyleOverride))]
    public sealed class TextStyleOverrideDrawer : PropertyDrawer
    {
        private static readonly string[] Fields =
        {
            "font", "sizeDegrees", "textColor", "colorTextBySpeaker", "backgroundColor", "backgroundOpacity",
        };

        private const float ToggleWidth = 18f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return line * (Fields.Length + 1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float step = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect row = new Rect(position.x, position.y, position.width, lineHeight);

            EditorGUI.LabelField(row, label);
            EditorGUI.indentLevel++;
            Rect indented = EditorGUI.IndentedRect(row);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            foreach (string field in Fields)
            {
                row.y += step;
                indented.y = row.y;

                SerializedProperty toggle = property.FindPropertyRelative("override" + char.ToUpperInvariant(field[0]) + field.Substring(1));
                SerializedProperty value = property.FindPropertyRelative(field);

                Rect toggleRect = new Rect(indented.x, row.y, ToggleWidth, lineHeight);
                Rect valueRect = new Rect(indented.x + ToggleWidth, row.y, indented.width - ToggleWidth, lineHeight);

                EditorGUI.PropertyField(toggleRect, toggle, GUIContent.none);
                using (new EditorGUI.DisabledScope(!toggle.boolValue))
                {
                    EditorGUI.PropertyField(valueRect, value);
                }
            }

            EditorGUI.indentLevel = indent - 1;
            EditorGUI.EndProperty();
        }
    }
}
