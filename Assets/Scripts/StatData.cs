using UnityEngine;

[CreateAssetMenu(fileName = "StatData", menuName = "Scriptable Objects/StatData")]
public class StatData : ScriptableObject
{
    [Header("Base Stats")]
    public new string name = "New Entity";
    public float maxHealth = 100f;
    public float attackPower = 10f;
    public float defense = 5f;
    public float moveSpeed = 6f;
    public float attackCooldown = 1f;
    
    
}
