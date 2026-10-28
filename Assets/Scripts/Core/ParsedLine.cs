using System.Collections.Generic;

namespace ImmersiveSubtitles.Core
{
    public sealed class ParsedLine
    {
        public string displayText; // <> tags stripped out except <i> <b> <u> 
        public string countedText; // displayText parsed (tags removed and whitespace collapsed) 
        public string speakerId; 
        public bool isAsync;
        public double? delaySeconds;
        public bool newTurn;
        public List<string> warnings = new(); // unknown/malformed tags
    }
}