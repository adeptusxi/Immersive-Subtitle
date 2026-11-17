using UnityEngine;

public class BeamMotion : MonoBehaviour
{
    [SerializeField, Min(0.01f), Tooltip("m/s")] private float speed = 3f;
    [SerializeField, Min(0f)] private float scaleDuration = 0.2f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Min(0f)] private float scaleOutDelay = 0.5f;
    [SerializeField, Min(0f)] private float destroyDelay = 5f;

    private enum Phase { Moving, Holding, ScalingOut, Waiting }

    private Vector3 baseScale;
    private Vector3 target;
    private bool launched;
    private Phase phase;
    private float phaseTime;
    private float scaleProgress; 

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    public void Launch(Vector3 from, Vector3 to)
    {
        transform.position = from;
        Vector3 direction = to - from;
        if (direction.sqrMagnitude > 1e-6f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        target = to;
        launched = true;
        SetPhase(Phase.Moving);
        scaleProgress = 0f;
        ApplyScale();
    }

    private void Update()
    {
        if (!launched)
        {
            return;
        }

        float delta = Time.deltaTime;
        phaseTime += delta;
        float scaleStep = scaleDuration > 0f ? delta / scaleDuration : 1f;

        switch (phase)
        {
            case Phase.Moving:
                scaleProgress = Mathf.Min(1f, scaleProgress + scaleStep);
                transform.position = Vector3.MoveTowards(transform.position, target, speed * delta);
                if (transform.position == target)
                {
                    SetPhase(Phase.Holding);
                }
                break;

            case Phase.Holding:
                // keep growing so a short trip still reaches full size
                scaleProgress = Mathf.Min(1f, scaleProgress + scaleStep);
                if (phaseTime >= scaleOutDelay)
                {
                    SetPhase(Phase.ScalingOut);
                }
                break;

            case Phase.ScalingOut:
                // from wherever scale in got to, so it doesn't pop
                scaleProgress = Mathf.Max(0f, scaleProgress - scaleStep);
                if (scaleProgress <= 0f)
                {
                    SetPhase(Phase.Waiting);
                }
                break;

            case Phase.Waiting:
                if (phaseTime >= destroyDelay)
                {
                    Destroy(gameObject);
                }
                break;
        }

        ApplyScale();
    }

    private void SetPhase(Phase next)
    {
        phase = next;
        phaseTime = 0f;
    }

    private void ApplyScale()
    {
        transform.localScale = baseScale * scaleCurve.Evaluate(scaleProgress);
    }
}
