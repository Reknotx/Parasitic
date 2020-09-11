using UnityEngine;

[CreateAssetMenu(fileName = "Character Stats", menuName = "Character Statistics", order = 53)]
public class CharacterStats : ScriptableObject
{
    public int Health;
    public int BaseAttack;
    public int BaseDefense;
    public int Movement;

    [Range(0.0f, .4f)]
    public float Dexterity;
}
