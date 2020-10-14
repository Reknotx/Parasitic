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
using UnityEngine.Events;

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
    Done
}

/// <summary>
/// Enum representing if the unit is defending this round.
/// </summary>
public enum DefendingState
{
    NotDefending,
    Defending,
}

/// <summary>
/// Enum of the shape of actions
/// </summary>
public enum ActionShape
{
    Flood, //diamond shape that can move around corners (used for movement)
    Diamond,
    Square,
    Cross
}

#pragma warning disable CS0649
public class Humanoid : MonoBehaviour, IMove, IStatistics
{
    /// <summary> The range of the normal attack. </summary>
    public int AttackRange { get; set; } 

    /// <summary> The max health of this unit. </summary>
    protected int _maxHealth;

    /// <summary> Health of the unit. </summary>
    int _health;
    
    /// <summary> Health properties. Sets health UI</summary>
    public int Health
    {
        get { return _health; }
        set
        {
            _health = Mathf.Clamp(value, 0, _maxHealth);
            if(healthText)
                healthText.text = _health + "/" + _maxHealth;
            //Update the image fill
            if(healthBar)
                healthBar.value = (float)_health / (float)_maxHealth;
        }
    }

    public int MaxHealth { get { return _maxHealth; } }

    /// <summary>Attack of the unit. </summary>
    public int AttackStat { get; set; }

    /// <summary>Defense of the unit.</summary>
    public int DefenseStat { get; set; }

    /// <summary>Movement value of the unit. </summary>
    public int MovementStat { get; set; }

    /// <summary>Dexterity (or dodge chance) of the unit.</summary>
    public float DexterityStat { get; set; }

    /// <summary> The shape of the unitys attack </summary>
    public ActionShape AttackShape = ActionShape.Diamond;

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

    [Space]
    [Header("The text component representing this unit's health.")]
    /// <summary> The text component representing this unit's Health. </summary>
    public Text healthText;

    [Space]
    [Header("The text component representing how much damage was dealt to this unit.")]
    /// <summary> The text component representing how much damage was dealt to this unit. </summary>
    public Text damageText;

    [Space]
    [Header("The graphical slider representing our health bar.")]
    /// <summary> The graphical slider representing our health bar. </summary>
    public Slider healthBar;

    /// <summary> time it takes to switch directions </summary>
    float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    /// <summary> The state of the humanoid in combat. </summary>
    public HumanoidState State { get; set; }


    /// <summary> States whether or not this unit is defending this round. </summary>
    public DefendingState DefendState { get; set; }

    public AudioClip attackSoundEffect;
    public AudioClip damagedSoundEffect;

    public AudioSource audioSource;
    
    public virtual void Start()
    {
        _maxHealth = _baseStats.Health;
        AttackStat = _baseStats.BaseAttack;
        DefenseStat = _baseStats.BaseDefense;
        MovementStat = _baseStats.Movement;
        DexterityStat = _baseStats.Dexterity;
        AttackRange = _baseStats.AttackRange;
        if (healthText == null) { healthText = GetComponentInChildren<Text>(); }
        if (healthBar == null) { healthBar = GetComponentInChildren<Slider>(); }
        Health = _maxHealth;

        
        /*if(healthText)
        healthText.text = Health + "/" + _maxHealth;

        if(healthBar)
        healthBar.value = 1f;*/

        currentTile = MapGrid.Instance.TileFromPosition(transform.position);
        currentTile.occupied = true;

        State = HumanoidState.Idle;
        DefendState = DefendingState.NotDefending;
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


    /// <summary>
    /// Movement coroutine that moves the unit along the grid.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    IEnumerator MoveCR(List<Tile> path)
    {
        Vector3 p0;
        Vector3 p1;
        Vector3 p01;
        float timeStart;
        Vector3 direction;
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
                direction = (p1 - p0).normalized;
                LookInDirection(direction);
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
        HealingTileCheck();
    }

    void HealingTileCheck()
    {
        if (this is Player && currentTile.tileEffect == TileEffect.Healing && currentTile.remainingCooldown <= 0)
        {
            Health = Health + (int)currentTile.TileBoost(TileEffect.Healing);
            print("Healing Tile used");
            currentTile.StartCooldown();
            CombatSystem.Instance.coolingTiles.Add(currentTile);
        }
    }

    protected void LookInDirection(Vector3 direction)
    {
        
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    /**
     * <summary>Deals damage to unit.</summary>
     * 
     * <param name="damage">Damage unit will take.</param>
     * 
     * <returns>True if unit is dead, false otherwise.</returns>
     */
    public bool TakeDamage(int damage, bool trueDamage = false)
    {
        if (this == null) return false;

        int damageDealt = damage;

        //If defending apply defense stat reduction.
        if (trueDamage == false)
        {
            if (DefendState == DefendingState.Defending)
            {
                print("Target unit was defending this round.");
                damageDealt -= DefenseStat + (int)currentTile.TileBoost(TileEffect.Defense);
                if (damageDealt <= 0) damageDealt = 0;
            }
            else //See if we can dodge the attack.
            {
                float chance = Random.Range(0.0f, 1.0f);

                if (chance <= DexterityStat + currentTile.TileBoost(TileEffect.Dodge))
                {
                    //Then dodge the attack.
                    damageDealt = 0;
                }
            }
        }
        
        Health -= damageDealt;
        
        StartCoroutine(ShowDamage(damageDealt));
        

        return Health <= 0 ? true : false;
    }

    /// <summary>
    /// When the incoming attack is dodged correctly we will activate the animation
    /// for this particular unit here.
    /// </summary>
    private void Dodge()
    {
        // Activate the dodging animation
    
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
        if (damage == 0)
        {
            damageText.text = "Miss";
        }
        else
        {
            damageText.text = damage.ToString();
        }
        yield return new WaitForSecondsRealtime(1.5f);
        damageText.text = "";
    }

    /// <summary> Plays the attacking sound effect for this unit. </summary>
    protected void PlayAttackSoundEffect()
    {
        audioSource.clip = attackSoundEffect;

        audioSource.Play();
    }

    /// <summary> Plays the damaged sound effect for this unit when they take damage. </summary>
    protected void PlayDamagedSoundEffect()
    {
        audioSource.clip = damagedSoundEffect;

        audioSource.Play();
    }

    /// <summary>
    /// Advances the timer on the unit's buff/debuff clock.
    /// </summary>
    public virtual void AdvanceTimer()
    {
        List<StatusEffect> removeList = new List<StatusEffect>();

        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.ReduceDuration())
            {
                removeList.Add(effect);
            }
        }

        foreach (StatusEffect effect in removeList)
        {
            statusEffects.Remove(effect);
        }

        removeList.Clear();
    }

    /// <summary>
    /// Creates a taunted status effect on this unit.
    /// </summary>
    /// <param name="source">The source that caused the status effect.</param>
    /// <param name="target">The target of the status effect.</param>
    /// <param name="duration">How long teh status effect lasts. Has a default value of three rounds.</param>
    public void CreateTauntedStatusEffect(Humanoid source, Humanoid target, int duration = 3)
    {
        StatusEffect temp = new StatusEffect(StatusEffect.StatusEffectType.Taunted, 
                                            duration, 
                                            source, 
                                            target);
        AddEffectToList(temp);
    }

    /// <summary>
    /// Creates a attack up status effect on this unit.
    /// </summary>
    /// <param name="source">The source that caused the status effect.</param>
    /// <param name="target">The target of the status effect.</param>
    /// <param name="duration">How long teh status effect lasts. Has a default value of three rounds.</param>
    public void CreateAttackUpStatusEffect(Humanoid source, Humanoid target, int duration = 3)
    {
        StatusEffect temp = new StatusEffect(StatusEffect.StatusEffectType.AttackUp, 
                                            duration, 
                                            source, 
                                            target);
        AddEffectToList(temp);
    }

    /// <summary>
    /// Creates a attack down status effect on this unit.
    /// </summary>
    /// <param name="source">The source that caused the status effect.</param>
    /// <param name="target">The target of the status effect.</param>
    /// <param name="duration">How long teh status effect lasts. Has a default value of three rounds.</param>
    public void CreateAttackDownStatusEffect(Humanoid source, Humanoid target, int duration = 3)
    {
        AttackStat = AttackStat / 2;

        StatusEffect temp = new StatusEffect(StatusEffect.StatusEffectType.AttackDown, 
                                            duration, 
                                            source, 
                                            target);
        AddEffectToList(temp);
    }

    private void AddEffectToList(StatusEffect effect)
    {
        statusEffects.Add(effect);
        CombatSystem.Instance.SubscribeTimerUnit(this);
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

    public int GetNumOfStatusEffects()
    {
        return statusEffects.Count;
    }

    /// <summary>
    /// Returns a reference to the unit that caused this status effect.
    /// </summary>
    /// <param name="type">The type of status effect we wish to look for.</param>
    /// <returns></returns>
    public Humanoid GetSourceOfStatusEffect(StatusEffect.StatusEffectType type)
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.Type == type)
            {
                return effect.Source;
            }
        }

        return null;
    }

    /// <summary>
    /// Searches through the list of status effects to see if this unit has a status
    /// effect of a certain type on them currently.
    /// </summary>
    /// <param name="type">The type of status effect to search for.</param>
    /// <returns>True if that type of status effect is currently active, false otherwise.</returns>
    protected bool CheckForEffectOfType(StatusEffect.StatusEffectType type)
    {
        if (statusEffects.Count == 0) return false;

        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.Type == type) return true;
        }

        return false;
    }


    public class StatusEffect
    {
        public enum StatusEffectType
        {
            Taunted,
            AttackDown,
            AttackUp
        }

        int _duration;

        /// <summary> The target of the status Effect (a.k.a. this unit) </summary>
        public Humanoid Target { get; set; }

        /// <summary> Where the status effect came from. </summary>
        public Humanoid Source { get; set; }


        public StatusEffectType Type { get; set; }
        int Duration { get { return _duration; } }

        public StatusEffect(StatusEffectType type, int duration, Humanoid source, Humanoid target)
        {
            this.Type = type;
            _duration = duration;
            this.Target = target;
            this.Source = source;
        }

        public bool ReduceDuration()
        {
            _duration--;

            if (_duration == 0) { return true; }

            return false;
        }
    }
}
