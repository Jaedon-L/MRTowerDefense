using System;
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
    public event Action<EnemyPathFollower> OnFinished;

    private Animator _animator;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.acceleration *= accelerationMultiplier;
        _agent.speed *= speedMultiplier;
        _animator = GetComponent<Animator>(); 
    }

    /// <summary>
    /// Call immediately after Instantiate().
    /// </summary>
    public void SetPath(List<Vector3> path)
    {
        if (path == null || path.Count < 2)
        {
            Debug.LogError("[EnemyPathFollower] Path too short!");
            FinishAndDestroy();
            return;
        }

        _path = new List<Vector3>(path);
        // We start *at* index 0 (spawn), so first move is to index 1:
        _currentIndex = 1;
        _agent.SetDestination(_path[_currentIndex]);
    }

    void Update()
    {
        if (_path == null || _path.Count < 2) return;

        // Smoothly rotate toward movement direction
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

        // Have we reached (or nearly reached) our current waypoint?
        if (!_agent.pathPending && _agent.remainingDistance <= waypointTolerance)
        {
            // If there are more waypoints ahead…
            if (_currentIndex < _path.Count - 1)
            {
                _currentIndex++;
                _agent.SetDestination(_path[_currentIndex]);
            }
            else
            {
                // We were on the last waypoint → finish
                FinishAndDestroy();
            }
        }
    }

    private void FinishAndDestroy()
    {

        var playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
        OnFinished?.Invoke(this);
        OnFinished = null;
        Destroy(gameObject);
    }
    
    public void OnDestroy()
    {
        // In case something else destroys us early
        // _animator.SetTrigger("Die");
        OnFinished?.Invoke(this);
    }
    public void Die()
    {
        _animator.SetTrigger("Die");
        _agent.isStopped = true;
        Destroy(gameObject, 2);
    }

    void OnDrawGizmosSelected()
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
