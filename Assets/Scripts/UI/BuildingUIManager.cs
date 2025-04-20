using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
[Obsolete("This class is deprecated. Use RadialMenu instead.")]
public class BuildingUIManager : MonoBehaviour
{
    public GameObject buildingPanel;
    public TMP_Text buildingNameText;
    public Transform unitButtonContainer;
    public GameObject unitButtonPrefab;

    private Building currentBuilding;

    public void ShowBuildingUI(Building building)
    {
        currentBuilding = building;
        buildingPanel.SetActive(true);

        buildingNameText.text = building.buildingData.buildingName;

        // Clear old buttons
        foreach (Transform child in unitButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Spawn buttons for each unit
        foreach (var unit in building.buildingData.spawnableUnits)
        {
            GameObject buttonObj = Instantiate(unitButtonPrefab, unitButtonContainer);
            TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();
            btnText.text = unit.unitName;

            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(() => building.TrySpawnUnit(unit));
        }
    }

    public void HideBuildingUI()
    {
        buildingPanel.SetActive(false);
        currentBuilding = null;
    }
}
