using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// [RequireComponent(typeof(NavMeshAgent))]
public class PathProvider : MonoBehaviour
{
    [Header("Path Endpoints")]
    [Tooltip("Where the path should start (e.g. enemy spawn).")]
    public Transform startPoint;
    [Tooltip("Where the path should end (e.g. goal).")]
    public Transform endPoint;

    [Header("Result")]
    [Tooltip("The list of world-space points the AI should move through.")]
    public List<Vector3> PathPoints = new List<Vector3>();

    private NavMeshPath _navPath;

    void Awake()
    {
        _navPath = new NavMeshPath();
    }

    void Start()
    {
        RecalculatePath();
    }

    /// <summary>
    /// Call this manually if you ever move start/end at runtime and need a fresh path.
    /// </summary>
    public void RecalculatePath()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogWarning("PathProvider: Start or End point not set.");
            return;
        }

        // Calculate a new path
        bool success = NavMesh.CalculatePath(
            startPoint.position,
            endPoint.position,
            NavMesh.AllAreas,
            _navPath
        );

        if (!success || _navPath.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning("PathProvider: Failed to calculate a complete path.");
            PathPoints.Clear();
            return;
        }

        // Copy the corners into our list
        PathPoints.Clear();
        PathPoints.AddRange(_navPath.corners);
    }

    // Optional: visualize in the Editor
    void OnDrawGizmosSelected()
    {
        if (PathPoints == null || PathPoints.Count < 2) return;
        Gizmos.color = Color.green;
        for (int i = 1; i < PathPoints.Count; i++)
        {
            Gizmos.DrawLine(PathPoints[i - 1], PathPoints[i]);
        }
    }
}
