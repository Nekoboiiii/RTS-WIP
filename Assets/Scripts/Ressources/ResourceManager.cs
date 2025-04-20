using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public float metal;
    public float stone;
    public float wood;
    public float gold;
    public float food;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanAfford(UnitScriptableObject unit)
    {
        return
            metal >= unit.metalCost &&
            stone >= unit.stoneCost &&
            wood >= unit.woodCost &&
            gold >= unit.goldCost &&
            food >= unit.foodCost;
    }

    public void SpendResources(UnitScriptableObject unit)
    {
        metal -= unit.metalCost;
        stone -= unit.stoneCost;
        wood -= unit.woodCost;
        gold -= unit.goldCost;
        food -= unit.foodCost;
    }

    // Optional: Add a GainResources() method later if needed
}
