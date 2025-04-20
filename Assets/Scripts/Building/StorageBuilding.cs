using UnityEngine;

public class StorageBuilding : MonoBehaviour
{
    public int storageCapacity = 100;
    private int currentStorage = 0;

    public void StoreResource(int amount)
    {
        currentStorage += amount;
        if (currentStorage > storageCapacity)
        {
            currentStorage = storageCapacity;
            Debug.Log("Lager ist voll.");
        }
        Debug.Log($"Aktueller Lagerbestand: {currentStorage}");
    }
}