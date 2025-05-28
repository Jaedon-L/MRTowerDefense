using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPathFollower : MonoBehaviour
{
    [Header("Path Settings")]
    [Tooltip("How close to each point before moving to the next (meters)")]
    public float waypointTolerance = 0.1f;

    [Header("Movement Smoothing")]
    [Tooltip("How quickly the enemy turns to face its movement direction")]
    public float rotationSpeed = 5f;
    [Tooltip("Multiplier for agent acceleration (higher = snappier)")]
    public float accelerationMultiplier = 1f;
    [Tooltip("Multiplier for agent speed")]
    public float speedMultiplier = 1f;

    private List<Vector3> _path;
    private int _currentIndex;
    private NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        // Disable navmesh auto-rotation so we can handle it manually:
        _agent.updateRotation = false;

        // Apply multipliers:
        _agent.acceleration *= accelerationMultiplier;
        _agent.speed        *= speedMultiplier;
    }

    /// <summary>
    /// Call this immediately after instantiating the enemy.
    /// </summary>
    public void SetPath(List<Vector3> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogError("[EnemyPathFollower] Received empty path!");
            return;
        }

        _path = new List<Vector3>(path);
        _currentIndex = 0;
        MoveToNextPoint();
    }

    void Update()
    {
        if (_path == null) return;

        // 1) Smooth rotation toward movement direction
        Vector3 vel = _agent.velocity;
        if (vel.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(vel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // 2) Advance to next waypoint when close enough
        if (!_agent.pathPending && _agent.remainingDistance <= waypointTolerance)
        {
            _currentIndex++;
            if (_currentIndex < _path.Count)
                MoveToNextPoint();
            else
                OnPathComplete();
        }
    }

    private void MoveToNextPoint()
    {
        _agent.SetDestination(_path[_currentIndex]);
    }

    private void OnPathComplete()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (_path == null) return;
        Gizmos.color = Color.red;
        for (int i = _currentIndex; i < _path.Count; i++)
        {
            Gizmos.DrawSphere(_path[i], 0.05f);
            if (i > _currentIndex)
                Gizmos.DrawLine(_path[i - 1], _path[i]);
        }
    }
}
