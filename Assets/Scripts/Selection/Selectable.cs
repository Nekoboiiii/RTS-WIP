using UnityEngine;

public class Selectable : MonoBehaviour
{
    [HideInInspector] public Unit unit;
    [HideInInspector] public Building building;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        building = GetComponent<Building>();
    }

    public void Select()
    {
        //Debug.Log($"{name} Selected");

        if (building != null && building.buildingData != null)
        {
            // Do something building-specific if needed
        }
        else if (building != null)
        {
            Debug.LogWarning($"Building data is missing on {gameObject.name}");
        }

        // Visual feedback like highlighting can go here
    }

    public void Deselect()
    {
        //Debug.Log($"{name} Deselected");

        // Remove highlight or other feedback here
    }
}
