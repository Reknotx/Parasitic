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
    #region The base starting stats of this unit.
    /// The base stats of this unit. Used to reset individual stats when certain things happen.

    ///<summary> The unit's base movement stat.</summary>
    protected int _baseMovement;

    /// <summary> The unit's base attack stat. </summary>
    protected int _baseAttack;

    /// <summary> The unit's base defense stat. </summary>
    protected int _baseDefense;

    /// <summary> The unit's base attack range. </summary>
    protected int _baseRange;
    #endregion

    #region Unit Combat Stats
    /// <summary> The private variable for the normal attack range. </summary>
    private int _attackRange;

    /// <summary> The range of the normal attack. </summary>
    public int AttackRange 
    {
        get
        {
            return _attackRange;
        } 

        set
        {
            _attackRange = Mathf.Clamp(value, 0, 10);
        }
    } 

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

    /// <summary> Public property returning the unit's max health value, keeping it as read only. </summary>
    public int MaxHealth { get { return _maxHealth; } }

    /// <summary>Attack of the unit. </summary>
    public int AttackStat { get; set; }

    /// <summary>Defense of the unit.</summary>
    public int DefenseStat { get; set; }

    /// <summary>Movement value of the unit. </summary>
    public int MovementStat { get; set; }

    /// <summary>Dexterity (or dodge chance) of the unit.</summary>
    public float DexterityStat { get; set; }
    
    /// <summary>XP Dropped when this Unit Dies</summary>
    public int XpDrop { get; set; }

    /// <summary>
    /// The move speed modifier, used only by player's for when there are no enemies
    /// visible in the level.
    /// </summary>
    protected int moveSpeedModifier;
    #endregion

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

    /// <summary> Indicates if the unit was damaged this turn. </summary>
    /// The turn means the phase on which the unit takes damage.
    /// Used for certain abilities.
    [HideInInspector] public bool damagedThisTurn = false;

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

    #region Health UI
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
    #endregion

    /// <summary> time it takes to switch directions </summary>
    float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    //[Header("This refers to the transform of the parent object for this unit.")]
    /// <summary> The transform of the parent of this unit. </summary>
    /// This is needed because the animations will put the character up in the Y due to how they
    /// were set up and how we have originally set up the characters themselves
    /// 
    [HideInInspector] public Transform parentTransform;

    /// <summary> The state of the humanoid in combat. </summary>
    public HumanoidState State { get; set; }


    /// <summary> States whether or not this unit is defending this round. </summary>
    public DefendingState DefendState { get; set; }

    /// <summary> Indicates if the unit is turning towards the target. </summary>
    protected bool IsTurning { get; set; } = false;

    [HideInInspector] public AudioClip attackSoundEffect;
    [HideInInspector] public AudioClip damagedSoundEffect;

    [HideInInspector] public AudioSource audioSource;

    public Animator animatorController;

    public bool AnimationComplete { get; set; } = false;

    #region Particle Systems
    public ParticleSystem attackParticle;

    public ParticleSystem defendParticle;

    [SerializeField] protected ParticleSystem activeParticle;
    #endregion
    /// <summary>
    /// Sets the animation complete parameter, used through animation events.
    /// </summary>
    /// <param name="value">States if the animation is complete or not.</param>
    public void SetAnimationComplete(bool value)
    {
        AnimationComplete = value;
        //if (attackParticle != null)
        //    attackParticle.Stop();

        //if (activeParticle != null)
        //{
        //    activeParticle.Stop();
        //}
        //activeParticle = null;
    }

    /// <summary>
    /// Sets the active particle that we wish to execute;
    /// </summary>
    /// <param name="particle">The particle system we want to play.</param>
    public void SetActiveParticle(ParticleSystem particle)
    {
        activeParticle = particle;
        activeParticle.Play();
    }

    public void DeactivateActiveParticle()
    {
        ///Possibly might need this in the future if things get funky.
    }

    /// <summary>
    /// Activates the attack particle system if it exists.
    /// </summary>
    protected void ActivateAttackParticle()
    {
        if (attackParticle != null)
        {
            SetActiveParticle(attackParticle);
        }
    }

    public virtual void Start()
    {
        if (transform.parent != null && transform.parent.CompareTag("UnitHolder"))
        {
            parentTransform = transform.parent;
        }
        else
        {
            parentTransform = this.transform;
        }

        if (attackParticle != null)
            attackParticle.Stop();

        _maxHealth = _baseStats.Health;

        _baseAttack = _baseStats.BaseAttack;
        AttackStat = _baseStats.BaseAttack;

        _baseDefense = _baseStats.BaseDefense;
        DefenseStat = _baseStats.BaseDefense;

        _baseMovement = _baseStats.Movement;
        MovementStat = _baseStats.Movement;

        DexterityStat = _baseStats.Dexterity;

        _baseRange = _baseStats.AttackRange;
        AttackRange = _baseStats.AttackRange;

        XpDrop = _baseStats.XPDropOnDeath;

        Health = _maxHealth;

        currentTile = MapGrid.Instance.TileFromPosition(parentTransform.position);
        currentTile.occupied = true;

        State = HumanoidState.Idle;
        DefendState = DefendingState.NotDefending;
        currentTile.occupant = this;
        TileRange = MapGrid.Instance.FindTilesInRange(currentTile, MovementStat);

        HasMoved = false;
        HasAttacked = false;

        
    }

    #region Movement
    /// <summary>
    /// Begins the movement coroutine for moving on map.
    /// </summary>
    /// <param name="path">The path the unit will take.</param>
    public virtual void Move(List<Tile> path, bool bypassRangeCheck = false)
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
        if (this is Player)
        {
            CombatSystem.Instance.DeactivateCombatButtons();
        }

        List<Tile> untraveledPath = new List<Tile>(path);
        Vector3 p0;
        Vector3 p1;
        Vector3 p2;
        Vector3 p01;
        Vector3 p12;
        float timeStart;
        Vector3 direction;
        float unitHeight = parentTransform.position.y - (currentTile.slope ? MapGrid.Instance.tileHeight / 2f : 0) - currentTile.Elevation;
        foreach (Tile tile in path)
        {
            timeStart = Time.time;
            moving = true;
            //get the position of the tile the unit is starting on
            p0 = currentTile.ElevatedPos();


            //get the positon of the tile to move to
            p2 = tile.ElevatedPos();

            // set the y position to be that of the moving unit
            p0 = new Vector3(p0.x, unitHeight + p0.y + (currentTile.slope ? MapGrid.Instance.tileHeight/2f : 0), p0.z);

            p2 = new Vector3(p2.x, unitHeight + p2.y + (tile.slope ? MapGrid.Instance.tileHeight / 2f : 0), p2.z);
            
            if(currentTile.slope && tile.slope && currentTile.level == tile.level)
            {
                p1 = new Vector3((p2.x - p0.x) / 2 + p0.x, tile.Elevation + MapGrid.Instance.tileHeight / 2f + unitHeight, (p2.z - p0.z) / 2 + p0.z);
            }
            else if (currentTile.level < tile.level)
            {
                p1 = new Vector3((p2.x - p0.x) / 2 + p0.x, tile.Elevation + unitHeight, (p2.z - p0.z) / 2 + p0.z);
            }
            else
            {
                p1 = new Vector3((p2.x - p0.x) / 2 + p0.x, currentTile.Elevation + unitHeight, (p2.z - p0.z) / 2 + p0.z);
            }
            

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
                float u = (Time.time - timeStart) / (tileCrossTime/2);
                if (u >= 1)
                {
                    u = 1;
                    moving = false;
                }
                direction = (p1 - p0).normalized;
                LookInDirection(direction);
                p01 = (1 - u) * p0 + u * p1;
                parentTransform.position = p01;
                if (this is Enemy)
                {
                    EnemyPath.Instance.DrawPath(untraveledPath,
                                                parentTransform.position - Vector3.up * unitHeight,
                                                p1 - Vector3.up * unitHeight);
                }

                yield return new WaitForFixedUpdate();
            }
            timeStart = Time.time;
            moving = true;
            while (moving)
            {
                float u = (Time.time - timeStart) / (tileCrossTime/2);
                if (u >= 1)
                {
                    u = 1;
                    moving = false;
                }
                direction = (p2 - p1).normalized;
                LookInDirection(direction);
                p12 = (1 - u) * p1 + u * p2;
                parentTransform.position = p12;
                if (this is Enemy)
                {
                    EnemyPath.Instance.DrawPath(untraveledPath, parentTransform.position - Vector3.up * unitHeight);
                }

                yield return new WaitForFixedUpdate();
            }
            untraveledPath.RemoveAt(0);
        }
        //TileRange = MapGrid.Instance.FindTilesInRange(currentTile, Movement);
        HasMoved = true;

        if (animatorController != null)
        {
            animatorController.SetBool("IsWalking", false);
        }

        CharacterSelector.Instance.unitMoving = false;
        HealingTileCheck();
        if (this is Enemy)
        {
            EnemyPath.Instance.HidePath();
        }
        else if (this is Mage mage)
        {
            mage.staffAndBookController.SetBool("IsWalking", false);
        }

        if (this is Player player && player.HasAttacked == false)
        {
            player.FindActionRanges();
            CombatSystem.Instance.ActivateCombatButtons();
        }

        //Check if player has reach objective / collected objective item
        if(this is Player && (WinConditions.Instance.CheckWinCondition(Condition.ReachArea) || WinConditions.Instance.CheckWinCondition(Condition.GetKeyItem)))
        {
            CombatSystem.Instance.GameWon();
        }
        else
        {
            CombatSystem.Instance.SetBattleState(BattleState.Idle);
            State = HumanoidState.Selected;
        }

        

    }
    #endregion

    #region Damage
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
        bool blocked = false;

        //If defending apply defense stat reduction.
        if (trueDamage == false)
        {
            if (DefendState == DefendingState.Defending)
            {
                //print("Target unit was defending this round.");
                damageDealt -= DefenseStat + (int)currentTile.TileBoost(TileEffect.Defense);
                if (damageDealt <= 0)
                {
                    damageDealt = 0;
                    blocked = true;
                }
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
        
        StartCoroutine(ShowDamage(damageDealt, blocked));

        return Health <= 0 ? true : false;
    }

    /// <summary>
    /// Displays "damage" for a short time
    /// </summary>
    /// <param name="damage"> Amount of Damage to Display</param>
    /// <returns></returns>
    IEnumerator ShowDamage(int damage, bool blocked = false)
    {
        damageText.color = Color.red;
        if (this is Enemy enemy)
        {
            enemy.healthCanvas.SetActive(true);
        }
        else
        {
            enemy = null;
        }

        if (damage == 0)
        {
            if (blocked)
            {
                damageText.text = "Blocked";
            }
            else
            {
                damageText.text = "Miss";
            }
        }
        else
        {
            damageText.text = damage.ToString();
        }
        yield return new WaitForSecondsRealtime(1.5f);
        
        if (enemy != null)
        {
            enemy.healthCanvas.SetActive(false);
        }

        damageText.text = "";
    }
    #endregion

    /// <summary>
    /// Coroutine that turns the unit in the direction of their target.
    /// </summary>
    protected virtual IEnumerator LookToTarget()
    {
        IsTurning = true;
        Vector3 thisUnit = currentTile.transform.position;
        Vector3 targetUnit = CharacterSelector.Instance.SelectedTargetUnit.currentTile.transform.position;

        Vector3 angle = (targetUnit - thisUnit).normalized;

        while (LookInDirection(angle))
        {
            yield return new WaitForFixedUpdate();
        }
        IsTurning = false;
    }

    /// <summary>
    /// Checks to see if the player is on a healing tile.
    /// </summary>
    void HealingTileCheck()
    {
        if (this is Player player
            && currentTile.tileEffect == TileEffect.Healing
            && currentTile.remainingCooldown <= 0)
        {
            Health = Health + (int)currentTile.TileBoost(TileEffect.Healing);
            StartCoroutine(player.ShowHealText((int)currentTile.TileBoost(TileEffect.Healing)));
            print("Healing Tile used");
            currentTile.StartCooldown();
            CombatSystem.Instance.coolingTiles.Add(currentTile);
            if(Health <= 0)
            {
                CombatSystem.Instance.KillUnit(this);
            }
        }
    }

    /// <summary>
    /// Smoothly turns the unit towards the direction.
    /// </summary>
    /// <param name="direction">The direction we want to look in.</param>
    /// <returns>True if we are still turning, false if we've turned all the way.</returns>
    protected bool LookInDirection(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        float angle = Mathf.SmoothDampAngle(parentTransform.eulerAngles.y,
                                            targetAngle,
                                            ref turnSmoothVelocity,
                                            turnSmoothTime);

        float result = Mathf.Abs(targetAngle - angle);

        while(result >= 360)
        {
            result -= 360;
        }

        parentTransform.rotation = Quaternion.Euler(0f, angle, 0f);

        if (result < 0.1f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// When the incoming attack is dodged correctly we will activate the animation
    /// for this particular unit here.
    /// </summary>
    //private void Dodge()
    //{
    //    // Activate the dodging animation
    //}

    /// <summary>
    /// Temporarily raises the defense stat of this unit.
    /// </summary>
    public virtual void Defend()
    {
        DefendState = DefendingState.Defending;
        if (defendParticle != null)
        {
            defendParticle.Play();
        }
    }

    public void FindMovementRange()
    {
        TileRange = MapGrid.Instance.FindTilesInRange(currentTile, MovementStat + moveSpeedModifier);
    }

    // /// <summary>
    // /// Sets the unit's HasAttacked variable to true.
    // /// </summary>
    // protected void AttackComplete() { HasAttacked = true; }

    public void SetHumanoidState(HumanoidState state) { State = state; }

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
            if (effect.Type != StatusEffect.StatusEffectType.Taunted)
            {
                ResetSpecificStat(effect.Type);

                if (this is Mage mage)
                {
                    mage.AbilityTwoParticle.Stop();
                    mage.AbilityTwoParticle.Clear();
                }

            }

            statusEffects.Remove(effect);
        }

        removeList.Clear();
    }


    #region Status Effects
    private void AddEffectToList(StatusEffect effect)
    {
        statusEffects.Add(effect);
        CombatSystem.Instance.SubscribeTimerUnit(this);
    }

    public void ResetSpecificStat(StatusEffect.StatusEffectType stat)
    {
        switch (stat)
        {
            case StatusEffect.StatusEffectType.AttackDown:
            case StatusEffect.StatusEffectType.AttackUp:
                AttackStat = _baseAttack;
                break;

            case StatusEffect.StatusEffectType.DefenseDown:
            case StatusEffect.StatusEffectType.DefenseUp:
                DefenseStat = _baseDefense;
                break;

            case StatusEffect.StatusEffectType.MoveDown:
            case StatusEffect.StatusEffectType.MoveUp:
                MovementStat = _baseMovement;
                break;
        }
    }

    public int GetNumOfStatusEffects()
    {
        return statusEffects.Count;
    }

    public virtual void AddStatusEffect(StatusEffect effect)
    {
        statusEffects.Add(effect);
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
    /// Placeholder to remind me to do this tomorrow.
    /// </summary>
    public void ResetTheStatOnStatusEffectEnd()
    {

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
            AttackUp,
            MoveUp,
            MoveDown,
            DefenseUp,
            DefenseDown
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
    #endregion
}
