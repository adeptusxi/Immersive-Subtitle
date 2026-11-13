using System.Collections.Generic;

namespace ImmersiveSubtitles.Core
{
    public sealed class ParsedLine
    {
        public string displayText; // keeps <i> <b> <u>. effect tags become <link=x>. emotion tags become font/color/link. other <> tags stripped
        public string countedText; // displayText parsed (tags removed and whitespace collapsed) 
        public string speakerId; 
        public bool isAsync;
        public double? delaySeconds;
        public bool newTurn;
        public double volume = 1; // multiplier on Voice volume 
        public double textSize = 1; // multiplier on subtitle font size
        public List<string> warnings = new(); // unknown/malformed tags
    }
}