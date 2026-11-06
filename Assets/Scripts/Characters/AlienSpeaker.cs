using System.Collections;
using ImmersiveSubtitles.Speech;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class AlienSpeaker : Speaker
{
    // [SerializeField] private Animator animator; // xiao-TODO hook up animator once joyce is done with model
    
    [Header("Walk")]
    [SerializeField] private float navMeshSampleRadius = 5f;
    [SerializeField] private double settleSeconds = 0.5;

    [Header("Jump")]
    [SerializeField] private double jumpDuration = 0.5;
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private AnimationCurve jumpCurve = new(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));
    [SerializeField] private Transform rockLanding;
    [SerializeField] private double rockHopDuration = 0.8;
    [SerializeField] private float rockHopHeight = 1f;

    [Header("Echolocation")]
    [SerializeField] private ParticleSystem soundwaveVFX;
    [SerializeField] private float soundwaveVFXSpawnFrequency = 1f;
    [SerializeField] private Transform soundwaveVFXOrigin;
    private float soundwaveVFXLifetime = 5f;
     
    private static readonly int WalkTrigger = Animator.StringToHash("Walk");
    private static readonly int IdleTrigger = Animator.StringToHash("Idle");

    private NavMeshAgent agent;
    private NavMeshQueryFilter navFilter;
    private Vector3 homePosition;
    private Quaternion homeFacing;
    private float standOffset; // original height above nav mesh
    private Coroutine motion;
    private bool jumping;

    public bool IsWalking { get; private set; }

    private static double Now => NarrativeClock.Instance.Now;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        navFilter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };

        homePosition = transform.position;
        homeFacing = SpeakerRoot.rotation;

        if (TrySampleNavMesh(homePosition, out Vector3 ground))
        {
            standOffset = homePosition.y - ground.y;
        }
        
        agent.updateRotation = false;
        agent.baseOffset = standOffset;
        agent.enabled = false;
    }

    public void WalkTo(Vector3 pos)
    {
        StartWalk(pos, false);
    }

    public void WalkHome()
    {
        StartWalk(homePosition, true);
    }

    public void StartJumping()
    {
        StopMotion();
        jumping = true;
        motion = StartCoroutine(JumpRoutine());
    }

    public void StopJumping()
    {
        jumping = false;
    }

    public void PlaySoundwave(double seconds)
    {
        StartCoroutine(SoundwaveRoutine(seconds));
    }

    private void StartWalk(Vector3 target, bool returningHome)
    {
        StopMotion();
        IsWalking = true;
        motion = StartCoroutine(WalkRoutine(target, returningHome));
    }

    private void StopMotion()
    {
        if (motion != null)
        {
            StopCoroutine(motion);
        }

        if (IsWalking)
        {
            agent.enabled = false;
            // animator.SetTrigger(IdleTrigger);
        }

        IsWalking = false;
        jumping = false;
    }

    #region Routines

    private IEnumerator WalkRoutine(Vector3 target, bool returningHome)
    {
        // animator.SetTrigger(WalkTrigger);
        agent.enabled = true;

        if (!agent.isOnNavMesh || !TrySampleNavMesh(target, out Vector3 destination) || !agent.SetDestination(destination))
        {
            Debug.LogWarning($"[Subtitles] {name} found no nav mesh path to {target}");
            agent.enabled = false;
            // animator.SetTrigger(IdleTrigger);
            IsWalking = false;
            yield break;
        }

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            bool paused = NarrativeClock.Instance.IsPaused;
            agent.isStopped = paused;

            Vector3 direction = agent.velocity;
            direction.y = 0f;
            
            if (!paused && direction.sqrMagnitude > 0.0001f)
            {
                SpeakerRoot.rotation = Quaternion.RotateTowards(SpeakerRoot.rotation, Quaternion.LookRotation(direction), agent.angularSpeed * Time.deltaTime);
            }
            
            yield return null;
        }

        agent.enabled = false;

        if (returningHome)
        {
            foreach (object _ in Settle(homePosition, homeFacing, settleSeconds))
                yield return null;
        }

        // animator.SetTrigger(IdleTrigger);
        IsWalking = false;
    }

    private IEnumerator JumpRoutine()
    {
        double startAt = Now + Random.Range(0f, (float)jumpDuration);
        
        while (jumping && Now < startAt)
        {
            yield return null;
        }
        
        if (!jumping)
        {
            yield break;
        }
        
        // hop onto rock 
        Vector3 rockForward = rockLanding.forward;
        rockForward.y = 0f;
        Quaternion rockFacing = rockForward.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(rockForward) : SpeakerRoot.rotation;
        yield return Hop(rockLanding.position, rockHopDuration, rockHopHeight, jumpCurve, rockFacing);

        // hop up and down on rock
        while (jumping)
        {
            yield return Hop(rockLanding.position, jumpDuration, jumpHeight, jumpCurve);
        }

        // hop down from rock
        yield return Hop(homePosition, rockHopDuration, rockHopHeight, jumpCurve, homeFacing, reverse: true);
    }

    private IEnumerator SoundwaveRoutine(double seconds)
    {
        double interval = 1.0 / soundwaveVFXSpawnFrequency;
        double endAt = Now + seconds;
        double nextSpawnAt = Now;

        while (Now < endAt)
        {
            if (Now >= nextSpawnAt)
            {
                ParticleSystem wave = Instantiate(soundwaveVFX, soundwaveVFXOrigin.position, soundwaveVFXOrigin.rotation);
                ParticleSystem.MainModule main = wave.main;
                main.startColor = profile.color;
                Destroy(wave.gameObject, soundwaveVFXLifetime);
                
                nextSpawnAt += interval;
            }
            yield return null;
        }
    }
    
    // yielded from JumpRoutine as a nested coroutine
    private IEnumerator Hop(Vector3 landing, double duration, float height, AnimationCurve curve, Quaternion? facing = null, bool reverse = false)
    {
        Vector3 from = transform.position;
        Quaternion fromFacing = SpeakerRoot.rotation;
        double startedAt = Now;

        for (float t = 0f; t < 1f; t = (float)((Now - startedAt) / duration))
        {
            float eased = Mathf.SmoothStep(0f, 1f, t);
            Vector3 position = Vector3.Lerp(from, landing, eased);
            position.y += height * curve.Evaluate(reverse ? 1f - t : t);
            transform.position = position;
            if (facing.HasValue)
            {
                SpeakerRoot.rotation = Quaternion.Slerp(fromFacing, facing.Value, eased);
            }
            yield return null;
        }

        transform.position = landing;
        if (facing.HasValue)
        {
            SpeakerRoot.rotation = facing.Value;
        }
    }

    private IEnumerable Settle(Vector3 position, Quaternion facing, double duration)
    {
        Vector3 fromPosition = transform.position;
        Quaternion fromFacing = SpeakerRoot.rotation;
        double startedAt = Now;

        for (float t = 0f; t < 1f; t = (float)((Now - startedAt) / duration))
        {
            float eased = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(fromPosition, position, eased);
            SpeakerRoot.rotation = Quaternion.Slerp(fromFacing, facing, eased);
            yield return null;
        }

        transform.position = position;
        SpeakerRoot.rotation = facing;
    }

    #endregion

    private bool TrySampleNavMesh(Vector3 point, out Vector3 result)
    {
        bool found = NavMesh.SamplePosition(point, out NavMeshHit hit, navMeshSampleRadius, navFilter);
        result = hit.position;
        return found;
    }
}
