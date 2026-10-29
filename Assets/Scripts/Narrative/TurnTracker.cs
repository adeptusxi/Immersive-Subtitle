using System;

namespace ImmersiveSubtitles.Narrative
{
    public sealed class TurnTracker
    {
        private string lastSpeakerId;
        private string currentTurnId;
        private int index;
        private int counter;
        private bool breakPending;

        public TurnAssignment Assign(string speakerId, bool forceNew)
        {
            if (currentTurnId == null || !string.Equals(speakerId, lastSpeakerId, StringComparison.OrdinalIgnoreCase) || forceNew || breakPending)
            {
                counter++;
                currentTurnId = "turn-" + counter;
                index = 0;
            }
            else
            {
                index++;
            }
            lastSpeakerId = speakerId;
            breakPending = false;
            return new TurnAssignment { turnId = currentTurnId, indexInTurn = index };
        }
        
        public void MarkBreak()
        {
            breakPending = true;
        }
    }
}
