using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "RadialMenuEntry", menuName = "UI/Radial Menu Entry")]
public class RadialMenuEntry : ScriptableObject
{
    public Sprite icon;
    public string label;

    [Header ("Action Type")]
    public UnitScriptableObject unitToSpawn;
    public UnityEvent onClickFallback;
    
    public bool IsUnitSpawn => unitToSpawn != null;
}
