/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: Humanoid base class file.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enum representing the unit's current state in the system.
/// </summary>
public enum HumanoidState
{
    Idle,
    Selected,
    Moving,
    Targetting,
    Attacking,
    Defending,
    Done
}

#pragma warning disable CS0649
public class Humanoid : MonoBehaviour, IMove, IStatistics
{
    /// <summary> The range of the normal attack. </summary>
    public int AttackRange { get; set; } 

    /// <summary> The max health of this unit. </summary>
    private int _maxHealth;

    /// <summary> Health of the unit. </summary>
    public int Health { get; set; }

    public int MaxHealth { get { return _maxHealth; } }

    /// <summary>Attack of the unit. </summary>
    public int AttackStat { get; set; }

    /// <summary>Defense of the unit.</summary>
    public int DefenseStat { get; set; }

    /// <summary>Movement value of the unit. </summary>
    public int MovementStat { get; set; }

    /// <summary>Dexterity (or dodge chance) of the unit.</summary>
    public float DexterityStat { get; set; }

    /// <summary> Tile the unit currently occupies </summary>
    [HideInInspector] public Tile currentTile;

    /// <summary> Tiles the unit can move to </summary>
    [HideInInspector] public bool[,] TileRange { get; set; } 

    /// <summary> Tiles the unit can attack on </summary>
    [HideInInspector] public bool[,] AttackTileRange { get; set; }

    ///time it takes the player to move across a single tile
    public float tileCrossTime = 0.3f;
    /// <summary> Is unity currently moving along its path </summary>
    bool moving = false;

    /// <summary> The value representing the remaining time on the buff/debuff
    /// currently active on this unit. </summary>
    int buffTimer = 0;

    /// <summary>
    /// Refers to how many remaining actions the unit has left this turn. Useful for 
    ///overriding the system if necessary.
    /// </summary>
    [HideInInspector] public int RemainingActions = 2;

    /// <summary> Indicates that the unit has moved this turn. </summary>
    public bool HasMoved { get; set; }

    /// <summary> Indicates that the unit has attacked this turn. </summary>
    public bool HasAttacked { get; set; }

    /// <summary>The base stats of the unit.</summary>
    [SerializeField] private CharacterStats _baseStats;

    protected List<StatusEffect> statusEffects = new List<StatusEffect>();

    public Text healthText;
    public Text damageText;

    public Slider healthBar;

    /// <summary>
    /// The state of the humanoid in combat.
    /// </summary>
    public HumanoidState State { get; set; }
    
    public virtual void Start()
    {
        Health = _baseStats.Health;
        AttackStat = _baseStats.BaseAttack;
        DefenseStat = _baseStats.BaseDefense;
        MovementStat = _baseStats.Movement;
        DexterityStat = _baseStats.Dexterity;
        AttackRange = _baseStats.AttackRange;
        _maxHealth = Health;

        if (healthText == null) { healthText = GetComponentInChildren<Text>(); }
        if (healthBar == null) { healthBar = GetComponentInChildren<Slider>(); }
        if(healthText)
        healthText.text = Health + "/" + _maxHealth;

        if(healthBar)
        healthBar.value = 1f;

        currentTile = MapGrid.Instance.TileFromPosition(transform.position);
        currentTile.occupied = true;

        State = HumanoidState.Idle;
        currentTile.occupant = this;
        TileRange = MapGrid.Instance.FindTilesInRange(currentTile, MovementStat);

        HasMoved = false;
        HasAttacked = false;
    }

    /// <summary>
    /// Begins the movement coroutine for moving on map.
    /// </summary>
    /// <param name="path">The path the unit will take.</param>
    public virtual void Move(List<Tile> path)
    {
        if (path != null)
        {
            CharacterSelector.Instance.unitMoving = true;
            CombatSystem.Instance.SetBattleState(BattleState.PerformingAction);
            State = HumanoidState.Moving;
            StartCoroutine(MoveCR(path));
        }
    }

    IEnumerator MoveCR(List<Tile> path)
    {
        Vector3 p0;
        Vector3 p1;
        Vector3 p01;
        float timeStart;
        foreach (Tile tile in path)
        {


            timeStart = Time.time;
            moving = true;

            //get the position of the tile the unit is starting on
            p0 = currentTile.transform.position;


            //get the positon of the tile to move to
            p1 = tile.transform.position;

            // set the y position to be that of the moving unit
            p0 = new Vector3(p0.x, transform.position.y, p0.z);
            p1 = new Vector3(p1.x, transform.position.y, p1.z);

            //mark the starting tile as no longer occupied
            currentTile.occupied = false;
            currentTile.occupant = null;
            //change the current tile to the tile being moved to
            currentTile = tile;
            //mark it as occupied
            currentTile.occupied = true;
            currentTile.occupant = this;
            //interpolate between the two points
            while (moving)
            {
                float u = (Time.time - timeStart) / tileCrossTime;
                if (u >= 1)
                {
                    u = 1;
                    moving = false;
                }

                p01 = (1 - u) * p0 + u * p1;
                transform.position = p01;
                yield return new WaitForFixedUpdate();
            }
        }
        //TileRange = MapGrid.Instance.FindTilesInRange(currentTile, Movement);
        State = HumanoidState.Idle;
        HasMoved = true;

        CombatSystem.Instance.SetBattleState(BattleState.Idle);
        CharacterSelector.Instance.unitMoving = false;
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
        if (this == null) return false;

        Health -= damage;
        
        StartCoroutine(ShowDamage(damage));
        healthText.text = Health + "/" + _maxHealth;
        //Update the image fill
        

        healthBar.value = (float)Health / (float) _maxHealth;

        return Health <= 0 ? true : false;
    }

    public void FindMovementRange()
    {
        TileRange = MapGrid.Instance.FindTilesInRange(currentTile, MovementStat);
    }

    // /// <summary>
    // /// Sets the unit's HasAttacked variable to true.
    // /// </summary>
    // protected void AttackComplete() { HasAttacked = true; }

    public void SetHumanoidState(HumanoidState state) { State = state; }

    /// <summary>
    /// Displays "damage" for a short time
    /// </summary>
    /// <param name="damage"> Amount of Damage to Display</param>
    /// <returns></returns>
    IEnumerator ShowDamage(int damage)
    {
        damageText.text = damage.ToString();
        yield return new WaitForSecondsRealtime(1.5f);
        damageText.text = "";
    }

    /// <summary>
    /// Advances the timer on the unit's buff/debuff clock.
    /// </summary>
    public virtual void AdvanceTimer()
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.ReduceDuration())
            {
                statusEffects.Remove(effect);

                if (statusEffects.Count == 0)
                {
                    CombatSystem.Instance.UnsubscribeAlteredUnit(this);
                }
            }
        }
    }

    public void CreateTauntedStatusEffect()
    {
        StatusEffect temp = new StatusEffect(StatusEffect.StatusEffectType.Taunted, 3);
        AddEffectToList(temp);
    }

    public void CreateAttackUpStatusEffect()
    {
        StatusEffect temp = new StatusEffect(StatusEffect.StatusEffectType.AttackUp, 3);
        AddEffectToList(temp);
    }

    public void CreateAttackDownStatusEffect()
    {
        AttackStat = AttackStat / 2;

        StatusEffect temp = new StatusEffect(StatusEffect.StatusEffectType.AttackDown, 3);
        AddEffectToList(temp);
    }

    private void AddEffectToList(StatusEffect effect)
    {
        statusEffects.Add(effect);
        CombatSystem.Instance.SubscribeAlteredUnit(this);
    }

    public void ResetStats()
    {
        Health = _baseStats.Health;
        AttackStat = _baseStats.BaseAttack;
        DefenseStat = _baseStats.BaseDefense;
        MovementStat = _baseStats.Movement;
        DexterityStat = _baseStats.Dexterity;
        _maxHealth = Health;
    }


    protected class StatusEffect
    {
        public enum StatusEffectType
        {
            Taunted,
            AttackDown,
            AttackUp
        }

        int _duration;

        StatusEffectType type;
        int Duration { get { return _duration; } }

        public StatusEffect(StatusEffectType type, int duration)
        {
            this.type = type;
            _duration = duration;
        }

        public bool ReduceDuration()
        {
            _duration--;

            if (_duration == 0) { return true; }

            return false;
        }

        public StatusEffectType GetEffectType()
        {
            return type;
        }
    }
}
