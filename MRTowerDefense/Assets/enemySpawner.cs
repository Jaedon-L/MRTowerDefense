using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class enemySpawner : MonoBehaviour
{
    [Tooltip("Enemy prefab must have a NavMeshAgent + EnemyPathFollower")]
    public GameObject enemyPrefab;

    [Tooltip("Seconds between spawns")]
    public float spawnInterval = 2f;

    [Tooltip("Max distance for NavMesh.SamplePosition")]
    public float sampleDistance = 0.5f;

    [Tooltip("LayerMask for your table collider")]
    public LayerMask tableLayer;

    // store the *local* coordinates of each waypoint
    private List<Vector3> _localPoints = new List<Vector3>();

    // once we start, we'll build this world-space list
    private List<Vector3> _worldPoints = new List<Vector3>();

    void Awake()
    {
        // Capture each child's localPosition (relative to this spawner)
        foreach (Transform child in transform)
        {
            _localPoints.Add(child.localPosition);
        }

        if (_localPoints.Count < 2)
            Debug.LogWarning("[EnemySpawner] Need at least 2 child waypoints on the spawner.");
    }

    [ContextMenu("Start Spawning")]
    public void StartSpawn()
    {
        // 1) Build the world-space path
        _worldPoints.Clear();
        foreach (var localPt in _localPoints)
        {
            Vector3 worldPt = transform.TransformPoint(localPt);

            // 2) Snap onto NavMesh if possible
            if (NavMesh.SamplePosition(worldPt, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                _worldPoints.Add(hit.position);
            }
            else
            {
                // 3) Fallback to table‐raycast
                Ray down = new Ray(worldPt + Vector3.up, Vector3.down);
                if (Physics.Raycast(down, out RaycastHit rh, 2f, tableLayer))
                    _worldPoints.Add(rh.point);
                else
                    Debug.LogWarning($"[Spawner] Waypoint at {worldPt} off-navmesh & off-table; skipping.");
            }
        }

        // 4) Kick off spawning if we have a valid path
        if (_worldPoints.Count >= 2)
            StartCoroutine(SpawnLoop());
        else
            Debug.LogError("[EnemySpawner] Not enough valid points to spawn enemies.");
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        // Always spawn at the *first* world point
        Vector3 spawnPos = _worldPoints[0];
        var go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        var follower = go.GetComponent<EnemyPathFollower>();
        if (follower != null)
            follower.SetPath(_worldPoints);
        else
            Debug.LogError("[EnemySpawner] Prefab missing EnemyPathFollower!");
    }

    void OnDrawGizmosSelected()
    {
        // draw the *local* offsets in editor
        Gizmos.color = Color.gray;
        foreach (var lp in _localPoints)
        {
            Gizmos.DrawSphere(transform.TransformPoint(lp), 0.05f);
        }

        // draw the *world* path during play
        if (_worldPoints.Count >= 2)
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < _worldPoints.Count; i++)
                Gizmos.DrawLine(_worldPoints[i - 1], _worldPoints[i]);
        }
    }
}
