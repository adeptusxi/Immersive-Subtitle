using ImmersiveSubtitles.Narrative;
using UnityEngine;

namespace ImmersiveSubtitles.Scenes
{
    public class Scene3 : MonoBehaviour
    {
        [SerializeField] private string[] alienIDs = { "alien1", "alien2", "alien3", "alien4", "alien5" };
        [SerializeField] private Transform walkAwayTo;
        
        private void OnEnable()
        {
            GameEvents.Raised += HandleEvent;
        }

        private void OnDisable()
        {
            GameEvents.Raised -= HandleEvent;
        }
        
        private void HandleEvent(string eventId)
        {
            switch (eventId)
            {
                case "aliens_walk_away": 
                    OnAliensWalkAway(); 
                    break;
                case "aliens_walk_away_complete": 
                    OnAliensWalkAwayComplete(); 
                    break;
                case "aliens_walk_back": 
                    OnAliensWalkBack(); 
                    break;
                case "aliens_walk_back_complete": 
                    OnAliensWalkBackComplete(); 
                    break;
                case "aliens_jump": 
                    OnAliensJump(); 
                    break;
                case "aliens_stop_jump": 
                    OnAlientStopJump(); 
                    break;
                case "play_soundwave_vfx": 
                    OnPlaySoundwaveVfx(); 
                    break;
            }
        }
        
        // xiao-TODO implement 
        
        private void OnAliensWalkAway()
        {
        }

        private void OnAliensWalkAwayComplete()
        {
        }

        private void OnAliensWalkBack()
        {
        }

        private void OnAliensWalkBackComplete()
        {
        }

        private void OnAliensJump()
        {
        }

        private void OnAlientStopJump()
        {
        }

        private void OnPlaySoundwaveVfx()
        {
        }
    }
}
