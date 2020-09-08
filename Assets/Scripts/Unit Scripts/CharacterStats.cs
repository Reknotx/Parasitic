using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Stats", menuName = "Character Statistics", order = 53)]
public class CharacterStats : ScriptableObject
{
    public int Health;
    public int BaseAttack;
    public int BaseDefense;
    public int Movement;
    public float Dexterity;
}
