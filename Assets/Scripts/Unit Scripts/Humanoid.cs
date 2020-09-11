using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649
public class Humanoid : MonoBehaviour, IMove, IStatistics
{
    private int _maxHealth;

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

    public Text healthText;
    public Text damageText;

    public Slider healthBar;
    
    private void Start()
    {
        Health = _baseStats.Health;
        BaseAttack = _baseStats.BaseAttack;
        BaseDefense = _baseStats.BaseDefense;
        Movement = _baseStats.Movement;
        Dexterity = _baseStats.Dexterity;
        _maxHealth = Health;

        if (healthText == null) { healthText = GetComponentInChildren<Text>(); }
        if (healthBar == null) { healthBar = GetComponentInChildren<Slider>(); }

        healthText.text = Health + "/" + _maxHealth;

        healthBar.value = 1f;
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
        damageText.text = damage.ToString();
        healthText.text = Health + "/" + _maxHealth;
        //Update the image fill
        

        healthBar.value = (float)Health / (float) _maxHealth;

        return Health <= 0 ? true : false;
    }
}
