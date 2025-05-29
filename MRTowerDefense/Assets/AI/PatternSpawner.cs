using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using UnityEngine.Analytics;
// using Oculus.Interaction.UnityEvents;

public class PatternSpawner : MonoBehaviour
{
    [Header("Prefabs & Transforms")]
    public List<GameObject> enemyPrefab;
    public GameObject buttonPrefab;
    public Transform startPoint, endPoint, spawnButton;

    [Header("Path Pattern")]
    public float spawnInterval = 2f;
    public enum PatternType { ZigZag, SineWave }
    public PatternType pattern = PatternType.ZigZag;
    public int numSegments = 8;
    public float amplitude = 0.5f;
    public float frequency = 2f;  // only for sine

    [Header("Wave Settings")]
    [Tooltip("Define each wave: enemy count and spawn interval")]
    public List<Wave> waves = new List<Wave>();

    private HashSet<EnemyPathFollower> _aliveEnemies = new HashSet<EnemyPathFollower>();

    [System.Serializable]
    public class Wave
    {
        [System.Serializable]
        public class EnemyEntry
        {
            public GameObject prefab;
            public int count;
        }
        public List<EnemyEntry> enemies = new List<EnemyEntry>();
        public float spawnInterval = 1f;
        public float delayBeforeNextWave = 2f;

        [Tooltip("How many towers the player may build in this wave")]
        public int towerLimit = 1;

    }
    private bool _started = false;
    private GameObject _buttonGO;
    private bool _waveStartPressed = false;
    private int currentWaveIndex = 0;
    private Coroutine waveRoutine;

    [Header("Tower Build UI")]
    public UISpawner uiSpawner;



    private List<Vector3> _path;
    // void Start()
    // {
    //     SpawnButton();
    // }
    private void Start()
    {
        var playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.onDeath.AddListener(StopSpawningOnGameOver);
        }
        uiSpawner = FindObjectOfType<UISpawner>();
    }
    private void StopSpawningOnGameOver()
    {
        Debug.Log("Stopping spawner due to game over.");
        StopAllCoroutines(); // Stops RunWaves or SpawnLoop
        _aliveEnemies.Clear(); // Optional: clear enemy tracking
        _started = false;

        // Optionally disable button or input too
        if (_buttonGO != null)
        {
            _buttonGO.SetActive(false);
        }
    }
    [ContextMenu("Start Spawn")]
    private void OnWaveStartPressed()
    {
        _buttonGO.SetActive(false);
        _aliveEnemies.Clear();
        SpawnWave(currentWaveIndex);
    }

    private void SpawnWave(int idx)
    {
        if (idx >= waves.Count) return;
        var wave = waves[idx];


        // 1) Impose tower limit
        var ui = FindObjectOfType<UISpawner>();
        if (ui != null) ui.SetTowerLimitForWave(wave.towerLimit); ui.StartNextWave();
        

        // 2) Start spawning enemies for this wave
        StartCoroutine(WaveCoroutine(wave));
    }

    private IEnumerator WaveCoroutine(Wave wave)
    {
        GeneratePath();
        // spawn enemies
        foreach (var entry in wave.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                var go = Instantiate(entry.prefab, _path[0], Quaternion.identity);
                var pf = go.GetComponent<EnemyPathFollower>();

                pf.SetPath(_path);
                _aliveEnemies.Add(pf);
                pf.OnFinished += HandleFinished;

                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        yield return new WaitUntil(() => _aliveEnemies.Count == 0);

        if (wave.delayBeforeNextWave > 0)
            yield return new WaitForSeconds(wave.delayBeforeNextWave);

        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            ShowButtonForCurrentWave();
        }
        else
        {
            OnAllWavesComplete();
        }
    }

    private void HandleFinished(EnemyPathFollower pf)
    {
        pf.OnFinished -= HandleFinished;
        _aliveEnemies.Remove(pf);
    }

    private void ShowButtonForCurrentWave()
    {
        _buttonGO.SetActive(true);
    }

    private void OnAllWavesComplete()
    {
        Debug.Log("All waves done!");
        _buttonGO.SetActive(false);
        // victory UI here...
    }

    private void GeneratePath()
    {
        switch (pattern)
        {
            case PatternType.ZigZag:
                _path = PatternedPathGenerator.ZigZagPath(
                    startPoint.position, endPoint.position,
                    numSegments, amplitude);
                break;
            case PatternType.SineWave:
                _path = PatternedPathGenerator.SineWavePath(
                    startPoint.position, endPoint.position,
                    numSegments + 1, amplitude, frequency);
                break;
        }
    }
    // [ContextMenu("Start Spawn")]
    // public void StartSpawn()
    // {
    //     // _waveStartPressed = true;
    //     // StartCoroutine(DespawnButton());
    //     Invoke("DespawnButton", 1f);
    //     if (_started) return;
    //     waveRoutine = StartCoroutine(RunWaves());

    // }
    [ContextMenu("Spawn button")]
    public void SpawnButton()
    {
        // 2) Spawn the button under your UI canvas or container
        Transform buttonpos = FindObjectOfType<ButtonPos>().transform; 
        _buttonGO = Instantiate(buttonPrefab, buttonpos.transform);
        // Parent without altering local scale:
        // btnGO.transform.SetParent(spawnButton, false);

        // 3) Link the button’s OnSelect to the spawner
        var wrapper = _buttonGO.GetComponent<InteractableUnityEventWrapper>();
        if (wrapper != null)
        {
            // Assume “WhenSelect” is the UnityEvent for poke‐select
            wrapper.WhenSelect.AddListener(OnWaveStartPressed);
            Debug.Log("caught");
        }
        else
        {
            Debug.LogError("Button prefab missing InteractableUnityEventWrapper!");
        }
    }
}
