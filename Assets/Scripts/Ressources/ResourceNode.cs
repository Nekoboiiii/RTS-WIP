using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public string resourceType;
    public int resourceAmount = 100;

    public void Harvest (int amount)
    {
        if (resourceAmount > 0)
        {
            resourceAmount -= amount;
            Debug.Log($"{amount} {resourceType} harvested, Remaining: {resourceAmount}");
        }
    }

    public bool HasResources()
    {
        return resourceAmount > 0;
    }
}