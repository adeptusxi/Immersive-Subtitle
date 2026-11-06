using System;
using System.Collections;
using System.Collections.Generic;
using ImmersiveSubtitles.Narrative;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Scenes
{
    public class Scene3 : MonoBehaviour
    {
        [SerializeField] private string[] alienIds = { "alien1", "alien2", "alien3", "alien4", "alien5" };
        [SerializeField] private string[] walkAwayAlienIds = { "alien2" };
        [SerializeField] private Transform walkAwayTo;
        [SerializeField] private float soundwaveSeconds = 3;
        
        private float moveTimeoutSeconds = 20;

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
                case "alien2_walk_away":
                    OnAliensWalkAway();
                    break;
                case "alien2_walk_back":
                    OnAliensWalkBack();
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

        private void OnAliensWalkAway()
        {
            List<AlienSpeaker> aliens = FindAliens(walkAwayAlienIds);
            
            foreach (AlienSpeaker alien in aliens)
            {
                alien.WalkTo(walkAwayTo.position);
            }
            
            StartCoroutine(WaitForWalk(aliens, OnAliensWalkAwayComplete));
        }

        private void OnAliensWalkAwayComplete()
        {
            GameEvents.Raise("alien2_walk_away_complete");
        }

        private void OnAliensWalkBack()
        {
            List<AlienSpeaker> aliens = FindAliens(walkAwayAlienIds);
            
            foreach (AlienSpeaker alien in aliens)
            {
                alien.WalkHome();
            }
            
            StartCoroutine(WaitForWalk(aliens, OnAliensWalkBackComplete));
        }

        private void OnAliensWalkBackComplete()
        {
            GameEvents.Raise("alien2_walk_back_complete");
        }

        private void OnAliensJump()
        {
            foreach (AlienSpeaker alien in FindAliens(alienIds))
            {
                alien.StartJumping();
            }
        }

        private void OnAlientStopJump()
        {
            foreach (AlienSpeaker alien in FindAliens(alienIds))
            {
                alien.StopJumping();
            }
        }

        private void OnPlaySoundwaveVfx()
        {
            foreach (AlienSpeaker alien in FindAliens(alienIds))
            {
                alien.PlaySoundwave(soundwaveSeconds);
            }
        }

        private IEnumerator WaitForWalk(List<AlienSpeaker> aliens, Action onComplete)
        {
            double giveUpAt = NarrativeClock.Instance.Now + moveTimeoutSeconds;
            
            yield return null;
            while (aliens.Exists(alien => alien.IsWalking) && NarrativeClock.Instance.Now < giveUpAt)
            {
                yield return null;
            }
            
            onComplete();
        }

        private List<AlienSpeaker> FindAliens(string[] ids)
        {
            List<AlienSpeaker> aliens = new();
            
            foreach (string id in ids)
            {
                if (SpeakerRegistry.Global.TryGet(id, out SpeakerQueue queue))
                {
                    aliens.Add(SpeakerRegistry.Global.RootOf(queue.Profile).GetComponentInParent<AlienSpeaker>());
                }
                else
                {
                    Debug.LogError($"[Subtitles] no speaker '{id}' in scene");
                }
            }
            
            return aliens;
        }
    }
}
