using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomWalk : MonoBehaviour
{
    [SerializeField] float nextTargetDistance = 4f;
    [SerializeField] bool motionEnabled = true;

    NavMeshAgent agent;

    public bool MotionEnabled
    {
        get => motionEnabled;
        set
        {
            motionEnabled = value;
            ApplyMotionState();
        }
    }

    public void StartMotion() => MotionEnabled = true;

    public void StopMotion() => MotionEnabled = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }

        ApplyMotionState();

        if (!motionEnabled || agent.pathPending)
        {
            return;
        }

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            TrySetRandomDestination();
        }
    }

    private void ApplyMotionState()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = !motionEnabled;
        }
    }

    private void TrySetRandomDestination()
    {
        Vector2 direction = Random.insideUnitCircle.normalized;
        Vector3 target = transform.position + new Vector3(direction.x, 0f, direction.y) * nextTargetDistance;

        var filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, nextTargetDistance, filter))
        {
            agent.SetDestination(hit.position);
        }
    }
}
