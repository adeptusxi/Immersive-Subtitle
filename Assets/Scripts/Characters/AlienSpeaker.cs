using System.Collections;
using ImmersiveSubtitles.Core;
using ImmersiveSubtitles.Presentation;
using ImmersiveSubtitles.Speech;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class AlienSpeaker : Speaker
{
    [Header("Alien")]
    [SerializeField, Min(0)] private int alienIdx = 0;
    [SerializeField] private Animator animator;
    
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
    [SerializeField] private Vector2 soundwaveStartOffset = new(0f, 0.75f);
    private float soundwaveVFXLifetime = 5f;

    [Header("Beam")]
    [SerializeField] private GameObject beamPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource soundwaveAudio;
    [SerializeField] private AudioSource walkAudio;
    [SerializeField] private AudioSource jumpAudio;
    [SerializeField, Min(0.01f)] private float loopFadeSeconds = 0.1f;
    [SerializeField] private bool playJumpSoundOnHop = true; // xiao-TODO temp until animations are ready

    private static readonly int IdleTrigger = Animator.StringToHash("Idle");
    private static readonly int WalkTrigger = Animator.StringToHash("Walk");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int JumpLoopTrigger = Animator.StringToHash("JumpLoop");
    private static readonly int DanceTrigger = Animator.StringToHash("Dance");
    private static readonly int AlienIdxParam = Animator.StringToHash("AlienIdx"); // TODO: set this to alienIdx 

    private NavMeshAgent agent;
    private NavMeshQueryFilter navFilter;
    private Vector3 homePosition;
    private Quaternion homeFacing;
    private float standOffset; // original height above nav mesh
    private Coroutine motion;
    private bool jumping;
    private LoopingAudio soundwaveLoop;
    private LoopingAudio walkLoop;
    private int activeSoundwaves;
    private int currentAnimation = IdleTrigger;

    public bool IsWalking { get; private set; }

    private static double Now => NarrativeClock.Instance.Now;

    private void Awake()
    {
        animator.SetInteger(AlienIdxParam, alienIdx);
    }

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

        soundwaveLoop = new LoopingAudio(soundwaveAudio, loopFadeSeconds);
        walkLoop = new LoopingAudio(walkAudio, loopFadeSeconds);
        LoopingAudio.ConfigureSpatial(jumpAudio);
        
        BeamPulseCue.BeamRequested += HandleBeamRequested;
    }

    private void OnDestroy()
    {
        BeamPulseCue.BeamRequested -= HandleBeamRequested;
    }
    
    private void LateUpdate()
    {
        bool paused = NarrativeClock.Instance.IsPaused;
        soundwaveLoop.Tick(activeSoundwaves > 0, paused, Time.unscaledDeltaTime);
        walkLoop.Tick(IsWalking, paused, Time.unscaledDeltaTime);
        animator.speed = paused ? 0f : 1f;
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

    public void StartDancing()
    {
        PlayAnimation(DanceTrigger);
    }

    public void ResetToHome()
    {
        StopAllCoroutines();
        motion = null;
        jumping = false;
        IsWalking = false;
        activeSoundwaves = 0;
        agent.enabled = false;
        transform.position = homePosition;
        SpeakerRoot.rotation = homeFacing;
        PlayAnimation(IdleTrigger);
    }

    public void PlaySoundwave(double seconds)
    {
        StartCoroutine(SoundwaveRoutine(seconds));
    }

    public void PlayJumpSound()
    {
        if (NarrativeClock.Instance.IsPaused)
        {
            return;
        }

        jumpAudio.PlayOneShot(jumpAudio.clip);
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
        }

        PlayAnimation(IdleTrigger);
        IsWalking = false;
        jumping = false;
    }

    private void PlayAnimation(int trigger)
    {
        if (trigger == currentAnimation)
        {
            return;
        }

        animator.ResetTrigger(currentAnimation);
        currentAnimation = trigger;
        animator.SetTrigger(trigger);
    }

    #region Routines

    private IEnumerator WalkRoutine(Vector3 target, bool returningHome)
    {
        PlayAnimation(WalkTrigger);
        agent.enabled = true;

        if (!agent.isOnNavMesh || !TrySampleNavMesh(target, out Vector3 destination) || !agent.SetDestination(destination))
        {
            Debug.LogWarning($"[Subtitles] {name} found no nav mesh path to {target}");
            agent.enabled = false;
            PlayAnimation(IdleTrigger);
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

        PlayAnimation(IdleTrigger);
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
        PlayAnimation(JumpTrigger);
        yield return Hop(rockLanding.position, rockHopDuration, rockHopHeight, jumpCurve, rockFacing);

        // hop up and down on rock
        while (jumping)
        {
            PlayAnimation(JumpLoopTrigger);
            yield return Hop(rockLanding.position, jumpDuration, jumpHeight, jumpCurve);
        }

        // hop down from rock
        PlayAnimation(JumpTrigger);
        yield return Hop(homePosition, rockHopDuration, rockHopHeight, jumpCurve, homeFacing, reverse: true);
        PlayAnimation(IdleTrigger);
    }

    private IEnumerator SoundwaveRoutine(double seconds)
    {
        double interval = 1.0 / soundwaveVFXSpawnFrequency;
        double endAt = Now + seconds;
        double nextSpawnAt = Now;
        
        yield return new WaitForSeconds(Random.Range(soundwaveStartOffset.x, soundwaveStartOffset.y));

        activeSoundwaves++;

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

        activeSoundwaves--;
    }
    
    // yielded from JumpRoutine as a nested coroutine
    private IEnumerator Hop(Vector3 landing, double duration, float height, AnimationCurve curve, Quaternion? facing = null, bool reverse = false)
    {
        if (playJumpSoundOnHop)
        {
            PlayJumpSound();
        }

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

    private void HandleBeamRequested(SpeakerProfile speaker, Vector3 start, Vector3 target)
    {
        if (speaker != profile || beamPrefab == null)
        {
            return;
        }

        GameObject beam = Instantiate(beamPrefab, start, Quaternion.identity);
        if (!beam.TryGetComponent(out BeamMotion motion))
        {
            motion = beam.AddComponent<BeamMotion>();
        }

        motion.Launch(start, target);
    }

    private bool TrySampleNavMesh(Vector3 point, out Vector3 result)
    {
        bool found = NavMesh.SamplePosition(point, out NavMeshHit hit, navMeshSampleRadius, navFilter);
        result = hit.position;
        return found;
    }
}
