using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class FormationPreviewer : MonoBehaviour
{
    public GameObject previewMarkerPrefab;
    private List<GameObject> activeMarkers = new();

    public void ShowPreview(List<Vector2> positions)
    {
        Clear();

        foreach(var pos in positions)
        {
            var marker = Instantiate(previewMarkerPrefab, pos, Quaternion.identity);
            activeMarkers.Add(marker);
        }
    }

    public void Clear()
    {
        foreach (var marker in activeMarkers)
        {
            Destroy(marker);
        }
        activeMarkers.Clear();
    }
}
