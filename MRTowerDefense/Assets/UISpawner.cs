using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpawner : MonoBehaviour
{
    public GameObject TowerPrefab;
    public Transform XRRigPos;
    public int maxTowersThisWave = 1;
    private int towersSpawned = 0;

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
    }

    public void SetTowerLimitForWave(int maxAllowed)
    {
        maxTowersThisWave = maxAllowed;
        towersSpawned = 0;
    }
}
