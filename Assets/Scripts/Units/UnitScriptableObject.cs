using UnityEngine;

[CreateAssetMenu(fileName = "UnitScriptableObject", menuName = "Scriptable Objects/Units")]
public class UnitScriptableObject : ScriptableObject
{
   [Header("Unit Data")]
    public Sprite icon; // Icon of the unit in the Unit UI
    public GameObject prefab;
    public string unitName; // Name of the unit
    public string description; // Description of the unit

    [Header("Unit Stats")]
    public float maxHealth; // Max health of the unit
    public float maxMana; // Mana of the unit
    public float maxArmor; // Max armor of the unit
    public float meleeAttackDamage; // Damage of the melee attack
    public float meleeAttackRange; // Range of the melee attack
    public float rangedAttackDamage; // Damage of the ranged attack
    public float rangedAttackRange; // Range of the ranged attack
    public float attackCooldown; // Time between attacks aka attack speed
    public float movementSpeed; // Speed of the unit
    

    [Header("Costs")]
    public float spawnTime; // Time it takes a building to build the unit
    public float spawnAmount; // Amount of units to spawn at once
    public float metalCost;
    public float stoneCost;
    public float woodCost;
    public float goldCost;
    public float foodCost;
    
}
