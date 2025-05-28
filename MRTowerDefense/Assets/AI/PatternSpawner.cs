using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject buttonPrefab;
    public Transform startPoint, endPoint;
    public float spawnInterval = 2f;

    // [Header("Pattern Settings")]
    public enum PatternType { ZigZag, SineWave }
    public PatternType pattern = PatternType.ZigZag;
    public int numSegments = 8;
    public float amplitude = 0.5f;
    public float frequency = 2f;  // only for sine

    private List<Vector3> _path;
    void Start()
    {
        Instantiate(buttonPrefab); 
    }
    [ContextMenu("Start spawning")]
    public void StartSpawn()
    {
        {
            // Ensure path is generated
            switch (pattern)
            {
                case PatternType.ZigZag:
                    _path = PatternedPathGenerator
                        .ZigZagPath(startPoint.position, endPoint.position,
                                    numSegments, amplitude);
                    break;
                case PatternType.SineWave:
                    _path = PatternedPathGenerator
                        .SineWavePath(startPoint.position, endPoint.position,
                                      numSegments + 1, amplitude, frequency);
                    break;
            }
            StartCoroutine(SpawnLoop());
        }
    }
    public IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        var go = Instantiate(enemyPrefab, _path[0], Quaternion.identity);
        var follower = go.GetComponent<EnemyPathFollower>();
        follower.SetPath(_path);
    }
}
