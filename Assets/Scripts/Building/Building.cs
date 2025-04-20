using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingScriptableObject buildingData;
    public Transform spawnPoint; // Optional: a custom spawn point

    private UnitScriptableObject currentUnitToSpawn;
    private float spawnCooldown = 0f;
    private bool isSpawning = false;

    void Start()
    {
        if (buildingData.spawnableUnits != null && buildingData.spawnableUnits.Count > 0)
        {
            currentUnitToSpawn = buildingData.spawnableUnits[0];
        }
        else
        {
            Debug.LogWarning("No spawnable units assigned to this building!");
        }
    }

    void Update()
    {
        if (isSpawning && currentUnitToSpawn != null)
        {
            spawnCooldown += Time.deltaTime;
            if (spawnCooldown >= currentUnitToSpawn.spawnTime)
            {
                SpawnUnit(currentUnitToSpawn);
                spawnCooldown = 0f;
                isSpawning = false;
            }
        }
    }

    public void SetUnitToSpawn(UnitScriptableObject unit)
    {
        if (buildingData.spawnableUnits.Contains(unit))
        {
            currentUnitToSpawn = unit;
        }
        else
        {
            Debug.LogWarning("Selected unit is not spawnable by this building.");
        }
    }

    public void TrySpawnUnit(UnitScriptableObject unit)
    {
        Debug.Log($"Trying to spawn: {unit.unitName}");

    if (ResourceManager.Instance == null)
    {
        Debug.LogError("ResourceManager.Instance is null!");
        return;
    }

    if (!ResourceManager.Instance.CanAfford(unit))
    {
        Debug.Log("Not enough resources to spawn " + unit.unitName);
        return;
    }

    if (unit.prefab == null)
    {
        Debug.LogError("Unit prefab is missing for: " + unit.unitName);
        return;
    }

        Vector3 spawnPos = transform.position + Vector3.right * 1.5f;
        Instantiate(unit.prefab, spawnPos, Quaternion.identity);
        ResourceManager.Instance.SpendResources(unit);
        Debug.Log("Spawned: " + unit.unitName);
    }

    void SpawnUnit(UnitScriptableObject unit)
    {
        Vector3 spawnPos = spawnPoint ? spawnPoint.position : transform.position + Vector3.right * 1.5f;
        Instantiate(unit.prefab, spawnPos, Quaternion.identity);
    }
}
