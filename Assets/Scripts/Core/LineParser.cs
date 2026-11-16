using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ImmersiveSubtitles.Core
{
    public static class LineParser
    {
        // '<' + optional '/' + a letter + anything except '<>' + '>'.
        // e.g. "a < b" is NOT a tag 
        private static readonly Regex RichTextTag = new(@"</?[A-Za-z][^<>]*>", RegexOptions.Compiled);
        private static readonly Regex KeptTag = new(@"^</?[ibu]>$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EffectTag = new(@"^<(/?)([A-Za-z][A-Za-z0-9_+]*)>$", RegexOptions.Compiled);
        private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

        public static bool IsBlank(string rawText)
        {
            return string.IsNullOrWhiteSpace(rawText);
        }

        // emotions may be null 
        public static ParsedLine Parse(string rawText, IReadOnlyList<string> tags, EmotionCatalog emotions = null)
        {
            string trimmed = rawText.Trim();
            string pendingClose = null; // no nesting effect tags
            var line = new ParsedLine
            {
                displayText = RichTextTag.Replace(trimmed, tag => DisplayTag(tag, emotions, ref pendingClose)).Trim(),
                countedText = Whitespace.Replace(RichTextTag.Replace(trimmed, string.Empty), " ").Trim(),
            };

            if (tags != null)
            {
                foreach (string tag in tags)
                    ParseTag(tag, line);
            }
            return line;
        }

        private static string DisplayTag(Match tag, EmotionCatalog emotions, ref string pendingClose)
        {
            if (KeptTag.IsMatch(tag.Value))
            {
                return tag.Value;
            }

            Match effect = EffectTag.Match(tag.Value);
            if (!effect.Success)
            {
                return string.Empty;
            }

            if (effect.Groups[1].Length > 0)
            {
                string close = pendingClose ?? "</link>";
                pendingClose = null;
                return close;
            }

            string name = effect.Groups[2].Value;
            if (emotions == null)
            {
                return $"<link={name}>";
            }
            
            EmotionStyle styled = null;
            var parts = new List<string>();
            foreach (string part in name.Split('+'))
            {
                if (!emotions.TryGet(part, out EmotionStyle emotion))
                {
                    if (part.Length > 0)
                    {
                        parts.Add(part);
                    }
                    continue;
                }

                styled ??= emotion;
                if (emotion.effect != null && !string.IsNullOrEmpty(emotion.effect.effectTag))
                {
                    parts.Add(emotion.effect.effectTag);
                }
            }

            if (styled == null)
            {
                return $"<link={name}>";
            }

            var open = new StringBuilder();
            var closing = new StringBuilder();
            if (styled.font != null)
            {
                open.Append("<font=\"").Append(styled.font.name).Append("\">");
                closing.Insert(0, "</font>");
            }

            if (styled.overrideColor)
            {
                open.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(styled.color)).Append('>');
                closing.Insert(0, "</color>");
            }

            if (parts.Count > 0)
            {
                open.Append("<link=").Append(string.Join("+", parts)).Append('>');
                closing.Insert(0, "</link>");
            }

            pendingClose = closing.ToString();
            return open.ToString();
        }

        private static void ParseTag(string tag, ParsedLine line)
        {
            int colon = tag.IndexOf(':');
            string key = (colon >= 0 ? tag.Substring(0, colon) : tag).Trim().ToLowerInvariant();
            string value = colon >= 0 ? tag.Substring(colon + 1).Trim() : null;

            switch (key)
            {
                case "speaker":
                    if (string.IsNullOrEmpty(value))
                        line.warnings.Add($"tag '#{tag}' is missing a speaker id");
                    else
                        line.speakerId = value;
                    break;

                case "async":
                case "new":
                    if (!string.IsNullOrEmpty(value))
                    {
                        line.warnings.Add($"tag '#{tag}' takes no value");
                        break;
                    }
                    if (key == "async")
                        line.isAsync = true;
                    else
                        line.newTurn = true;
                    break;

                case "delay":
                    if (value != null
                        && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds)
                        && seconds >= 0 && !double.IsInfinity(seconds))
                        line.delaySeconds = seconds;
                    else
                        line.warnings.Add($"tag '#{tag}' needs a non-negative number of seconds");
                    break;

                case "volume":
                    if (value != null
                        && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double volume)
                        && volume >= 0 && !double.IsInfinity(volume))
                        line.volume = volume;
                    else
                        line.warnings.Add($"tag '#{tag}' needs a non-negative number");
                    break;

                case "size":
                    if (value != null
                        && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double size)
                        && size > 0 && !double.IsInfinity(size))
                        line.textSize = size;
                    else
                        line.warnings.Add($"tag '#{tag}' needs a positive number");
                    break;

                default:
                    line.warnings.Add($"unknown tag '#{tag}'");
                    break;
            }
        }
    }
}
