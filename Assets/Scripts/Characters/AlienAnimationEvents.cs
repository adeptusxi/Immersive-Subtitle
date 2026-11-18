using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class AlienAnimationEvents : MonoBehaviour
{
    private AlienSpeaker alien;

    private void Awake()
    {
        alien = GetComponentInParent<AlienSpeaker>();
    }

    public void PlayJumpSound()
    {
        alien.PlayJumpSound();
    }
}
