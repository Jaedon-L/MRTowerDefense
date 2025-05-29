using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UISpawner : MonoBehaviour
{
    public GameObject TowerPrefab;
    public Transform XRRigPos;

    public int maxTowersThisWave = 1;
    private int towersSpawned = 0;

    public TextMeshPro Tower1Num;


    void Start()
    {
        UpdateTowerUI(); 
    }

    [ContextMenu("Spawn Tower 1")]
    public void SpawnTower1()
    {
        if (towersSpawned >= maxTowersThisWave)
        {
            Debug.Log("Cannot build more towers this wave.");
            return;
        }

        Instantiate(TowerPrefab, XRRigPos.position, XRRigPos.rotation);
        towersSpawned++;

        UpdateTowerUI(); // Update after placing a tower
    }

    public void SetTowerLimitForWave(int maxAllowed)
    {
        maxTowersThisWave = maxAllowed;
        towersSpawned = 0;

        UpdateTowerUI(); // Reset UI at start of wave
    }

private void UpdateTowerUI()
{
    int remainingTowers = maxTowersThisWave - towersSpawned;
    Tower1Num.text = remainingTowers.ToString();

  
}
}
