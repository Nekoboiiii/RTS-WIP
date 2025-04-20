using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingScriptableObjectScript", menuName = "Scriptable Objects/Buildings")]
public class BuildingScriptableObject : ScriptableObject
{
   [Header("Building Data")]
    public string buildingName; // Name of the building
    public string description; // Description of the building
    public GameObject prefab;
    public Sprite icon; // Icon of the building in the Building UI
    public Sprite buildingUIIcon; // The UI to show when the building is selected
  
    [Header("Building Defensive/Offensive Stats")]
    public float maxHealth; // Max health of the building
    public float maxArmor; // Max armor of the building
    public float attackRange; // Range of the attack
    public float rangedAttackDamage; // Damage of the ranged attack
    public float meleeAttackDamage; // Damage of the melee attack
    public float attackCooldown; // Time between attacks aka attack speed

    [Header("Building Costs")]
    public float buildTime; // Time it takes a worker to build the building
    public float metalBuildCost; 
    public float stoneBuildCost;
    public float woodBuildCost;
    public float goldBuildCost;

    [Header("Building Produce")]
    public float produceTime; // Time it takes to produce the resource
    public float produceAmount; // Amount of resource produced at once
    // Ressource type to produce 

    [Header("Building Spawner")]
    
    public List<UnitScriptableObject> spawnableUnits; // The prefab of the unit to spawn
}
