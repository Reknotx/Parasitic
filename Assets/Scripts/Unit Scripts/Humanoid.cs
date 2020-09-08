using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649
public class Humanoid : MonoBehaviour, IMove, IStatistics
{
    [HideInInspector] public int Health { get; set; }
    [HideInInspector] public int BaseAttack { get; set; }
    [HideInInspector] public int BaseDefense { get; set; }
    [HideInInspector] public int Movement { get; set; }
    [HideInInspector] public float Dexterity { get; set; }
    [SerializeField] private CharacterStats _baseStats;

    private void Start()
    {
        Health = _baseStats.Health;
        BaseAttack = _baseStats.BaseAttack;
        BaseDefense = _baseStats.BaseDefense;
        Movement = _baseStats.Movement;
        Dexterity = _baseStats.Dexterity;
    }

    public virtual void Move()
    {
        throw new System.NotImplementedException();
    }

    /**
     * <summary>Deals damage to unit.</summary>
     * 
     * <param name="damage">Damage unit will take.</param>
     * 
     * <returns>True if unit is dead, false otherwise.</returns>
     */
    public bool TakeDamage(int damage)
    {
        Health -= damage;

        return Health <= 0 ? true : false;
    }
}
