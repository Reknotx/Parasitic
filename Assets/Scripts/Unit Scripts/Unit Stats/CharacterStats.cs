using UnityEngine;

[CreateAssetMenu(fileName = "Character Stats", menuName = "Character Statistics", order = 53)]
public class CharacterStats : ScriptableObject
{
    public int AttackRange;
    public int Health;
    public int BaseAttack;
    public int BaseDefense;
    public int Movement;

    [Range(0.0f, 0.4f)]
    public float Dexterity;
}
