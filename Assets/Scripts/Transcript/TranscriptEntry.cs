using System.Collections.Generic;
using System.Linq;
using ImmersiveSubtitles.Core;

namespace ImmersiveSubtitles.Transcript
{
    public enum TranscriptEntryType
    {
        Turn, 
        Choice
    }
    
    public sealed class TranscriptEntry
    {
        public TranscriptEntryType type;
        public bool interrupted; 
        
        // for Turn types only: 
        public SpeakerProfile speaker; 
        public string turnId;
        public List<SegmentInstance> segments = new(); 
        
        // for Choice types only: 
        public string text; 
        
        public string Paragraph
        {
            get
            {
                if (type == TranscriptEntryType.Turn)
                {
                    return string.Join(" ", segments.Select(segment => segment.text));
                }
                
                return text;
            }
        }
    }
}
