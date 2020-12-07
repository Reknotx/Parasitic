/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: The base player class 
 * 
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


#pragma warning disable CS0414
public abstract class Player : Humanoid, IPlayer
{
    /// <summary> ObjectiveZone the player currently occupies </summary>
    ObjectiveZone currentObjectiveZone = null;

    bool selected = false;
    //public Material defaultMat;
    ///// <summary> The material for the player when they are selected. </summary>
    //public Material selectedMat;

    #region Particles
    /// <summary> EXP Particle System that Is a Child of the Player Unit </summary>
    public ParticleSystem ExpParticle;

    /// <summary> The particle system that is played when player is selected. </summary>
    public ParticleSystem SelectedParticle;

    /// <summary> The particle system for the player's first ability. </summary>
    public ParticleSystem AbilityOneParticle;

    /// <summary> The particle system for the player's second ability. </summary>
    public ParticleSystem AbilityTwoParticle;

    /// <summary> The particle system to be played when the player gets healed. </summary>
    public ParticleSystem HealParticle;
    #endregion

    #region Ability Variables
    /// <summary> Range of player's first ability. </summary>
    [Header("The range of the player's first ability.")]
    public int AbilityOneRange;

    [Space]
    /// <summary> Range of player's second ability. </summary>
    [Header("The range of the player's second ability.")]
    public int AbilityTwoRange;

    [Space]
    [Header("The cooldown of the player's first ability.")]
    /// <summary> The cooldown of the player's first ability. </summary>
    public int AbilityOneCooldown;

    [Space]
    [Header("The cooldown of the player's second ability.")]
    /// <summary> The cooldown of the player's second ability. </summary>
    public int AbilityTwoCooldown;

    /// <summary> The remaining cooldown on ability one. </summary>
    int _remainingAbilityOneCD;

    /// <summary> The remaining cooldown on ability two.  </summary>
    int _remainingAbilityTwoCD;

    [Space(5)]
    /// <summary> The sprites of the player's normal attack.</summary>
    public Sprite[] NormalAttackSprites = new Sprite[5];
    [Space(5)]
    /// <summary> The sprites of the player's first ability.</summary>
    public Sprite[] Ability1Sprites = new Sprite[5];
    [Space(5)]
    /// <summary> The sprites of the player's second ability.</summary>
    public Sprite[] Ability2Sprites = new Sprite[5];

    [Space(5)]
    public Sprite[] UpgradeToggleSprites = new Sprite[3];

    /// <summary> Tile range of the player's first ability. </summary>
    [HideInInspector] public bool[,] AbilityOneTileRange { get; set; }

    /// <summary> Tile range of the player's second ability. </summary>
    [HideInInspector] public bool[,] AbilityTwoTileRange { get; set; }

    /// <summary> Public property to get the remaining cooldown of ability 1. </summary>
    public int RemainingAbilityOneCD { get { return _remainingAbilityOneCD; } }

    /// <summary> Public property to get the remaining cooldown of ability2. </summary>
    public int RemainingAbilityTwoCD { get { return _remainingAbilityTwoCD; } }
    #endregion

    #region Abstract functions
    /// <summary> Abstract method for player ability one.</summary>
    public abstract void AbilityOne(Action callback);
    /// <summary> Abstract method for player ability two.</summary>
    public abstract void AbilityTwo(Action callback);
    /// <summary> Abstract method for player normal attack.</summary>
    public abstract void NormalAttack(Action callback);

    protected abstract IEnumerator NormalAttackCR(Action callback);
    protected abstract IEnumerator AbilityOneCR(Action callback);
    protected abstract IEnumerator AbilityTwoCR(Action callback);

    protected abstract void AttackUpgradeOne();
    protected abstract void AttackUpgradeTwo();
    protected abstract void AbilityOneUpgradeOne();
    protected abstract void AbilityOneUpgradeTwo();
    protected abstract void AbilityTwoUpgradeOne();
    protected abstract void AbilityTwoUpgradeTwo();

    public abstract void ProcessUpgrade(Abilities abilityToUpgrade);
    #endregion


    public override void Start()
    {
        //defaultMat = GetComponent<MeshRenderer>().material;
        //if (selectedMat == null) selectedMat = Resources.Load<Material>("SelectedMat");
        if (SelectedParticle != null) SelectedParticle.Stop();
        moveSpeedModifier = MovementStat;
        base.Start();
    }


    #region Selection/Deselection
    /// <summary>
    /// Called when the player is selected. Sets up the game system to display
    /// data specific to the player unit.
    /// </summary>
    public void UnitSelected()
    {
        //print("Player selected");
        //GetComponent<MeshRenderer>().material = selectedMat;
        SelectedParticle.Play();
        State = HumanoidState.Selected;

        CombatSystem.Instance.ActivateCombatButtons();
        CombatSystem.Instance.SetPlayer(this);
        selected = true;

        FindMovementRange();

        if (!HasMoved)
            CharacterSelector.Instance.BoarderLine.SetActive(true);

        Upgrades.Instance.upgradesMenuToggle.SetActive(true);
        

        SpriteState st;
        Upgrades.Instance.upgradesMenuToggle.GetComponent<Image>().sprite = UpgradeToggleSprites[0];
        st.highlightedSprite = UpgradeToggleSprites[1];
        st.pressedSprite = UpgradeToggleSprites[2];

        Upgrades.Instance.upgradesMenuToggle.GetComponent<Button>().spriteState = st;
    }

    /// <summary>
    /// Called when the player unit is deslected. Deactivates all coroutines that are
    /// running and hides information specific to that unit.
    /// </summary>
    public void UnitDeselected()
    {
        //print("Player deselected");
        StopAllCoroutines();
        //GetComponent<MeshRenderer>().material = defaultMat;
        SelectedParticle.Stop();
        SelectedParticle.Clear();
        State = HumanoidState.Idle;
        CombatSystem.Instance.SetPlayer(null);
        CombatSystem.Instance.DeactivateCombatButtons();
        selected = false;
        CharacterSelector.Instance.BoarderLine.SetActive(false);
        Upgrades.Instance.upgradesMenuToggle.SetActive(false);
    }
    #endregion

    #region Animation Executers
    ///Note: All animation triggers need to follow this naming convention for 
    ///simplistic implementation and programming reasons.

    /// <summary> Executes the normal attack animation of this player. </summary>
    protected void AttackAnim()
    {
        animatorController.SetTrigger("CastAttack");
        CombatSystem.Instance.SetBattleState(BattleState.PerformingAction);
        
    }

    /// <summary> Triggers the ability one animation for this player. </summary>
    protected void AbilityOneAnim()
    {
        animatorController.SetTrigger("CastAbilityOne");
        CombatSystem.Instance.SetBattleState(BattleState.PerformingAction);
        
    }

    /// <summary> Triggers the ability two animation for this player. </summary>
    protected void AbilityTwoAnim()
    {
        animatorController.SetTrigger("CastAbilityTwo");
        CombatSystem.Instance.SetBattleState(BattleState.PerformingAction);
        
    }
    #endregion

    #region Particle Functions
    #region Particle Activators
    /// <summary>
    /// Activates the particle effect for ability one if it exists.
    /// </summary>
    public void ActivateAbilityOneParticle()
    {
        if (AbilityOneParticle != null)
        {
            SetActiveParticle(AbilityOneParticle);
        }
    }

    /// <summary>
    /// Activates the particle effect for ability two if it exists.
    /// </summary>
    public void ActivateAbilityTwoParticle()
    {
        if (AbilityTwoParticle != null)
        {
            SetActiveParticle(AbilityTwoParticle);
        }
    }
    #endregion

    #region Particle Deactivators
    protected void DeactivateAbilityOneParticle()
    {
        AbilityOneParticle.Stop();
    }

    protected void DeactivateAbilityTwoParticle()
    {
        AbilityTwoParticle.Stop();
    }
    #endregion
    #endregion

    /// <summary>
    /// Raises the defense stat of the player temporarily.
    /// </summary>
    //public void Defend()
    //{
    //    //print("Defending this round.");
    //    DefendState = DefendingState.Defending;
    //}

    /// <summary>
    /// Override of advance timer that also reduces the cooldown on abilities.
    /// </summary>
    public override void AdvanceTimer()
    {
        if (_remainingAbilityOneCD > 0)
        {
            _remainingAbilityOneCD--;
            CombatSystem.Instance.SetCoolDownText(this);
        }

        if (_remainingAbilityTwoCD > 0)
        {
            _remainingAbilityTwoCD--;
            CombatSystem.Instance.SetCoolDownText(this);
        }

        base.AdvanceTimer();
    }

    public override void Move(List<Tile> path, bool bypassRangeCheck = false)
    {
        if (animatorController != null)
            animatorController.SetBool("IsWalking", true); 
        base.Move(path, bypassRangeCheck);
    }

    #region Ability Functions
    /// <summary>
    /// Starts the cooldown of this unit's first ability.
    /// </summary>
    protected void StartAbilityOneCD()
    {
        _remainingAbilityOneCD = AbilityOneCooldown;
        CombatSystem.Instance.SetCoolDownText(this);
    }

    /// <summary>
    /// Starts the cooldown of this unit's second ability.
    /// </summary>
    protected void StartAbilityTwoCD()
    {
        _remainingAbilityTwoCD = AbilityTwoCooldown;
        CombatSystem.Instance.SetCoolDownText(this);
    }
    #endregion

    /// <summary> Find the tile range of the player's normal attack, first ability, and second ability. </summary>
    public void FindActionRanges()
    {
        AttackTileRange = MapGrid.Instance.FindTilesInRange(currentTile, AttackRange, true, AttackShape);
        AbilityOneTileRange = MapGrid.Instance.FindTilesInRange(currentTile, AbilityOneRange, true);
        AbilityTwoTileRange = MapGrid.Instance.FindTilesInRange(currentTile, AbilityTwoRange, true);
        //print("Ranges found");
    }

    /// <summary>
    /// Performs a simple heal on the player, healing them for 20% of their
    /// max health.
    /// </summary>
    public void Heal()
    {
        ///States if the archers mend ability is upgraded.
        bool archerAbility1U1 = Upgrades.Instance.IsAbilityUnlocked(Abilities.ability1Upgrade1, UnitToUpgrade.archer);

        float healPercent = archerAbility1U1 ? 0.3f : 0.2f;

        Health += Mathf.FloorToInt(MaxHealth * healPercent);

        HealParticle.Play();

        StartCoroutine(ShowHealText(Mathf.FloorToInt(MaxHealth * healPercent)));
    }

    /// <summary>
    /// Small little coroutine that displays the "damage" text for when
    /// a heal is received from the Archer. The text color is set to green
    /// for the moment and then immediately set back to red for damage
    /// text.
    /// </summary>
    /// <param name="amount">The amount of health that was gained from potion.</param>
    public IEnumerator ShowHealText(int amount)
    {
        damageText.color = Color.green;
        damageText.text = amount.ToString();
          
        yield return new WaitForSecondsRealtime(1.5f);

        damageText.text = "";
        damageText.color = Color.red;
    }

    /// <summary> Returns a vector 3 representation of the target's position. </summary>
    /// <returns>Vector 3 of target's position</returns>
    protected Vector3 GetTargetPos()
    {
        Transform targetPos = CharacterSelector.Instance.SelectedTargetUnit.parentTransform;

        Vector3 posV3 = new Vector3(targetPos.position.x, 1f, targetPos.position.z);

        return posV3;
    }

    /// <summary> Doubles the move speed of the player, storing it in a global variable. </summary>
    public void DoubleMoveSpeed()
    {
        print("Doubling move speed of " + name);
        moveSpeedModifier = MovementStat;
    }

    /// <summary> Resets the speed modifier back to zero. </summary>
    public void SetMoveSpeedNormal()
    {
        print("Normalizing move speed of " + name);
        moveSpeedModifier = 0;
    }

    /// <summary>
    /// Sets currentObjectZone to zone entered or null if zone exited
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="entered"></param>
    public void InObjectiveZone(ObjectiveZone zone, bool entered)
    {
        if (entered)
        {
            currentObjectiveZone = zone;
        }
        else
        {
            currentObjectiveZone = null;
        }
    }

    public bool AtTargetObjective(ObjectiveZone target)
    {
        return target == currentObjectiveZone;
    }
}
