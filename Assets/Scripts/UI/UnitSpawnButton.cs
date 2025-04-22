using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Obsolete]

public class UnitSpawnButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Button button;

    private UnitScriptableObject unitData;

    public void Setup(UnitScriptableObject data, UnityAction onClick)
    {
        unitData = data;
        nameText.text = unitData.unitName;
        costText.text = $"Cost: {unitData.metalCost} Metal, {unitData.woodCost} Wood, {unitData.stoneCost} Stone, {unitData.goldCost} Gold";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }
}
