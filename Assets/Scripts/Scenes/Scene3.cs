using System;
using System.Collections;
using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Narrative;
using ImmersiveSubtitles.Speech;
using UnityEngine;

namespace ImmersiveSubtitles.Scenes
{
    public class Scene3 : MonoBehaviour
    {
        [SerializeField] private InkNarrativeRunnerBehaviour runner;
        [SerializeField] private string[] alienIds = { "alien1", "alien2", "alien3", "alien4", "alien5" };
        [SerializeField] private string[] walkAwayAlienIds = { "alien2" };
        [SerializeField] private Transform walkAwayTo;
        [SerializeField] private float soundwaveSeconds = 3;
        
        [Header("Objects")]
        [SerializeField] private GameObject largeRock;
        [SerializeField] private GameObject spaceship;
        [SerializeField] private GameObject largeRockNotice;
        [SerializeField] private GameObject spaceshipNotice;
        [SerializeField] private float objectRevealDuration = 0.5f;
        [SerializeField] private float noticeDelaySeconds = 3f;

        private const string SawLargeRockId = "saw_large_rock";
        private const string SawSpaceshipId = "saw_spaceship";

        private float moveTimeoutSeconds = 20;
        private Vector3 largeRockScale;
        private Vector3 spaceshipScale;

        private void Awake()
        {
            if (largeRock != null)
            {
                largeRockScale = largeRock.transform.localScale;
            }

            if (spaceship != null)
            {
                spaceshipScale = spaceship.transform.localScale;
            }

            HideObjects();
        }

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
                case "aliens_dance":
                    OnAliensDance();
                    break;
                case "play_soundwave_vfx":
                    OnPlaySoundwaveVfx();
                    break;
                case "show_large_rock":
                    OnShowLargeRock();
                    break;
                case "show_spaceship":
                    OnShowSpaceship();
                    break;
                case InkNarrativeRunner.RestartedEventId:
                    OnStoryRestarted();
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

        private void OnAliensDance()
        {
            foreach (AlienSpeaker alien in FindAliens(alienIds))
            {
                alien.StartDancing();
            }
        }

        private void OnPlaySoundwaveVfx()
        {
            foreach (AlienSpeaker alien in FindAliens(alienIds))
            {
                alien.PlaySoundwave(soundwaveSeconds);
            }
        }

        private void OnShowLargeRock()
        {
            Reveal(largeRock, largeRockScale);
            StartCoroutine(WatchUntilSeen(largeRock.transform, largeRockNotice, SawLargeRockId));
        }

        private void OnShowSpaceship()
        {
            Reveal(spaceship, spaceshipScale);
            StartCoroutine(WatchUntilSeen(spaceship.transform, spaceshipNotice, SawSpaceshipId));
        }

        private void OnStoryRestarted()
        {
            StopAllCoroutines();
            HideObjects();

            runner.Runner.SetVariable(SawLargeRockId, false);
            runner.Runner.SetVariable(SawSpaceshipId, false);

            foreach (AlienSpeaker alien in FindAliens(alienIds))
            {
                alien.ResetToHome();
            }
        }

        private void HideObjects()
        {
            if (largeRock != null)
            {
                largeRock.SetActive(false);
            }

            HideNotice(largeRockNotice);

            if (spaceship != null)
            {
                spaceship.SetActive(false);
            }

            HideNotice(spaceshipNotice);
        }

        private static void HideNotice(GameObject notice)
        {
            if (notice == null)
            {
                return;
            }

            notice.SetActive(false);
        }

        private void Reveal(GameObject target, Vector3 authoredScale)
        {
            if (target == null || target.activeSelf)
            {
                return;
            }

            StartCoroutine(ScaleIn(target.transform, authoredScale));
        }

        private IEnumerator ScaleIn(Transform target, Vector3 authoredScale)
        {
            double startedAt = NarrativeClock.Instance.Now;
            float progress = 0;

            target.localScale = Vector3.zero;
            target.gameObject.SetActive(true);

            while (progress < 1)
            {
                yield return null;

                progress = objectRevealDuration > 0
                    ? Mathf.Clamp01((float)((NarrativeClock.Instance.Now - startedAt) / objectRevealDuration))
                    : 1;
                target.localScale = authoredScale * Mathf.SmoothStep(0, 1, progress);
            }
        }
        
        private IEnumerator WatchUntilSeen(Transform target, GameObject notice, string eventId)
        {
            double noticeAt = NarrativeClock.Instance.Now + noticeDelaySeconds;

            yield return null;
            while (!Player.Instance.IsInFOV(target.position))
            {
                if (notice != null && !notice.activeSelf && NarrativeClock.Instance.Now >= noticeAt)
                {
                    notice.SetActive(true);
                }

                yield return null;
            }

            runner.Runner.SetVariable(eventId, true);
            GameEvents.Raise(eventId);
            HideNotice(notice);
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
