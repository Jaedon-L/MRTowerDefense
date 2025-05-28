
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavAgent : MonoBehaviour
{
    [Tooltip("Where this enemy should walk to")]
    public Transform targetPoint;

    private NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (targetPoint == null)
        {
            Debug.LogError($"[{name}] No targetPoint set on EnemyNavAgent.");
            enabled = false;
            return;
        }
        // Send the agent toward the opposite side
        _agent.SetDestination(targetPoint.position);
    }

    // Optional: visualize path in-scene
    void OnDrawGizmosSelected()
    {
        if (_agent == null || _agent.hasPath == false) return;
        Gizmos.color = Color.red;
        var path = _agent.path;
        for (int i = 1; i < path.corners.Length; i++)
        {
            Gizmos.DrawLine(path.corners[i-1], path.corners[i]);
        }
    }
}
