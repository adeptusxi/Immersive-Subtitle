namespace ImmersiveSubtitles.Speech
{
    public sealed class SegmentRequest
    {
        public string displayText;
        public string countedText;
        public string turnId;
        public int indexInTurn;
        public double rate;
        public double delaySeconds;
        public double volume = 1;
        public double textSize = 1;
    }
}
