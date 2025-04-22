using UnityEngine;

public class Selectable : MonoBehaviour
{
    [HideInInspector] public Unit unit;
    [HideInInspector] public Building building;

    // Visual feedback components (e.g., material for outline or highlight)
    private Renderer objectRenderer;
    private Color originalColor;
    private Color selectedColor = Color.yellow; // Or any highlight color you prefer

    private void Awake()
    {
        unit = GetComponent<Unit>();
        building = GetComponent<Building>();
        objectRenderer = GetComponent<Renderer>();

        // Set original color if renderer exists (for visual feedback)
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    public void Select()
    {
        Debug.Log($"{name} Selected");

        // If there is a building, handle building-specific logic
        if (building != null && building.buildingData != null)
        {
            // Handle building-specific selection logic
        }
        else if (building != null)
        {
            Debug.LogWarning($"Building data is missing on {gameObject.name}");
        }

        // Provide visual feedback for selection (e.g., change color)
        if (objectRenderer != null)
        {
            objectRenderer.material.color = selectedColor;
        }
    }

    public void Deselect()
    {
        Debug.Log($"{name} Deselected");

        // Remove visual feedback for deselection (reset color)
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }

    public void MoveTo(Vector2 position)
    {
        if (unit != null)
        {
            unit.MoveTo(position);
        }
        else
        {
            Debug.LogWarning("Attempted to move a selectable without a unit component.");
        }
    }
}
