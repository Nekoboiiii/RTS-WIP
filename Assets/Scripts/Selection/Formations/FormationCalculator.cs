using System.Collections.Generic;
using UnityEngine;

public class FormationCalculator
{
    public static List<Vector2> CalculateFormationPositions(
        Vector2 center,
        int totalUnits,
        int unitsPerLine,
        float? customSpacing = null // Optional dynamic spacing
    )
    {
        List<Vector2> positions = new();
        if (totalUnits == 0 || unitsPerLine == 0) return positions;

        float spacing = customSpacing ?? GetDefaultSpacing(); // Apply dynamic spacing
        int rows = Mathf.CeilToInt((float)totalUnits / unitsPerLine);
        int unitIndex = 0;

        for (int row = 0; row < rows; row++)
        {
            int unitsInThisRow = Mathf.Min(unitsPerLine, totalUnits - unitIndex);
            float rowWidth = (unitsInThisRow - 1) * spacing;

            for (int col = 0; col < unitsInThisRow; col++)
            {
                float xOffset = (col * spacing) - (rowWidth / 2f);
                float yOffset = (row * spacing) - ((rows - 1) * spacing / 2f); // Vertical centering

                Vector2 pos = center + new Vector2(xOffset, yOffset);
                positions.Add(pos);
                unitIndex++;
            }
        }

        return positions;
    }

    public static List<Vector2> CalculateOffsetsForUnits(
        int totalUnits,
        int unitsPerLine,
        float? customSpacing = null // Optional dynamic spacing
    )
    {
        List<Vector2> offsets = new();
        if (totalUnits == 0 || unitsPerLine == 0) return offsets;

        float spacing = customSpacing ?? GetDefaultSpacing(); // Apply dynamic spacing
        int rows = Mathf.CeilToInt((float)totalUnits / unitsPerLine);
        int unitIndex = 0;

        for (int row = 0; row < rows; row++)
        {
            int unitsInThisRow = Mathf.Min(unitsPerLine, totalUnits - unitIndex);
            float rowWidth = (unitsInThisRow - 1) * spacing;

            for (int col = 0; col < unitsInThisRow; col++)
            {
                float xOffset = (col * spacing) - (rowWidth / 2f);
                float yOffset = (row * spacing) - ((rows - 1) * spacing / 2f); //  Vertical centering

                offsets.Add(new Vector2(xOffset, yOffset));
                unitIndex++;
            }
        }

        return offsets;
    }

    // Dynamically calculates a default spacing (will be improved per unit type)
    private static float GetDefaultSpacing()
    {
        float unitSize = 1f; // Set based on unit visuals (will be dynamic later)
        return unitSize * 1.5f;
    }
}
