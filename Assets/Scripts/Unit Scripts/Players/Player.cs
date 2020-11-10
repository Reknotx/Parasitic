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
    protected enum AttackType
    {
        NormalAttack,
        AbilityOne,
        AbilityTwo
    }

    bool selected = false;
    //public Material defaultMat;
    ///// <summary> The material for the player when they are selected. </summary>
    //public Material selectedMat;

    #region Particles
    // EXP Particle System that Is a Child of the Player Unit
    public ParticleSystem ExpParticle;

    public ParticleSystem SelectedParticle;

    public ParticleSystem AbilityOneParticle;

    public ParticleSystem AbilityTwoParticle;
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
    public int AbilityOneCooldown;

    [Space]
    [Header("The cooldown of the player's second ability.")]
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

    /// <summary> Tiles ability1 affects </summary>
    [HideInInspector] public bool[,] AbilityOneTileRange { get; set; }
    /// <summary> Tiles ability 1 affects </summary>
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

    [Space]
    [HideInInspector] public AudioClip abilityOneSoundEffect;
    [HideInInspector] public AudioClip abilityTwoSoundEffect;

    public override void Start()
    {
        //defaultMat = GetComponent<MeshRenderer>().material;
        //if (selectedMat == null) selectedMat = Resources.Load<Material>("SelectedMat");
        if (SelectedParticle != null) SelectedParticle.Stop();
        base.Start();
    }


    #region Selection/Deselection
    public void UnitSelected()
    {
        //print("Player selected");
        //GetComponent<MeshRenderer>().material = selectedMat;
        SelectedParticle.Play();
        State = HumanoidState.Selected;

        CombatSystem.Instance.ActivateCombatButtons();
        CombatSystem.Instance.SetPlayer(this);
        selected = true;

        if (!HasMoved)
            CharacterSelector.Instance.BoarderLine.SetActive(true);

        Upgrades.Instance.upgradesMenuToggle.SetActive(true);
        Upgrades.Instance.ShowUpgradeNotification();

        SpriteState st;
        Upgrades.Instance.upgradesMenuToggle.GetComponent<Image>().sprite = UpgradeToggleSprites[0];
        st.highlightedSprite = UpgradeToggleSprites[1];
        st.pressedSprite = UpgradeToggleSprites[2];

        Upgrades.Instance.upgradesMenuToggle.GetComponent<Button>().spriteState = st;
    }

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
        Upgrades.Instance.ClearUpgradeNotification();
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

    }

    protected void DeactivateAbilityTwoParticle()
    {

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

    /// <summary> Play's the sound effect for this player's first ability. </summary>
    protected void PlayAbilityOneSoundEffect()
    {
        audioSource.clip = abilityOneSoundEffect;

        audioSource.Play();
    }

    /// <summary> Play's the sound effect for this player's second ability. </summary>
    protected void PlayAbilityTwoSoundEffect()
    {
        audioSource.clip = abilityTwoSoundEffect;

        audioSource.Play();
    }
    #endregion

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
    }

    /// <summary>
    /// Returns a vector 3 representation of the target's position.
    /// </summary>
    /// <returns>Vector 3 of target's position</returns>
    protected Vector3 GetTargetPos()
    {
        Transform targetPos = CharacterSelector.Instance.SelectedTargetUnit.parentTransform;

        Vector3 posV3 = new Vector3(targetPos.position.x, 1f, targetPos.position.z);

        return posV3;
    }
}
