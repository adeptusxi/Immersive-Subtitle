using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ImmersiveSubtitles.Core
{
    public static class LineParser
    {
        // '<' + optional '/' + a letter + anything except '<>' + '>'.
        // e.g. "a < b" is NOT a tag 
        private static readonly Regex RichTextTag = new(@"</?[A-Za-z][^<>]*>", RegexOptions.Compiled);
        private static readonly Regex KeptTag = new(@"^</?[ibu]>$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

        public static bool IsBlank(string rawText)
        {
            return string.IsNullOrWhiteSpace(rawText);
        }

        public static ParsedLine Parse(string rawText, IReadOnlyList<string> tags)
        {
            string trimmed = rawText.Trim();
            var line = new ParsedLine
            {
                displayText = RichTextTag.Replace(trimmed, m => KeptTag.IsMatch(m.Value) ? m.Value : string.Empty).Trim(),
                countedText = Whitespace.Replace(RichTextTag.Replace(trimmed, string.Empty), " ").Trim(),
            };

            if (tags != null)
            {
                foreach (string tag in tags)
                    ParseTag(tag, line);
            }
            return line;
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
