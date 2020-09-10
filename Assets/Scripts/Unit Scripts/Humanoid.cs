using UnityEngine;

#pragma warning disable CS0649
public class Humanoid : MonoBehaviour, IMove, IStatistics
{
    /// <summary> Health of the unit. </summary>
    [HideInInspector] public int Health { get; set; }

    /// <summary>Attack of the unit. </summary>
    [HideInInspector] public int BaseAttack { get; set; }

    /// <summary>Defense of the unit.</summary>
    [HideInInspector] public int BaseDefense { get; set; }

    /// <summary>Movement value of the unit. </summary>
    [HideInInspector] public int Movement { get; set; }

    /// <summary>Dexterity (or dodge chance) of the unit.</summary>
    [HideInInspector] public float Dexterity { get; set; }

    /// <summary>The base stats of the unit.</summary>
    [SerializeField] private CharacterStats _baseStats;

    private void Start()
    {
        Health = _baseStats.Health;
        BaseAttack = _baseStats.BaseAttack;
        BaseDefense = _baseStats.BaseDefense;
        Movement = _baseStats.Movement;
        Dexterity = _baseStats.Dexterity;
    }

    public virtual void Move(Transform start, Transform target)
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
