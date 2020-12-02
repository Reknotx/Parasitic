/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: Archer class file.
 */

using System;
using System.Collections;
using UnityEngine;
#pragma warning disable IDE0020 // Use pattern matching

public class Archer : Player
{
    /// <summary> The animtor controler for the Archer's accessories. </summary>
    public Animator ArcherAccController;

    /// <summary> Indicates if the Archer's eagle eye ability is active. </summary>
    private bool hasTrueDamage = false;

    /// <summary> Public variable telling us if the projectile has hit the target yet. </summary>
    [HideInInspector] public bool potionHitTarget = false, arrowHitTarget = false;

    /// <summary> The singleton instance of the Archer. </summary>
    public static Archer Instance;

    /// <summary> The splash particle system for the potion when it's hit the target. </summary>
    public ParticleSystem potionSplash;

    /// <summary> The potion game object. </summary>
    public GameObject potion;

    /// <summary> The arrow game object. </summary>
    public GameObject arrow;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }


    public override void Start()
    {
        healthBar = CombatSystem.Instance.archerHealthSlider;
        healthText = CombatSystem.Instance.archerHealthText;
        base.Start();
    }

    #region Normal Attack
        /// <summary>
        /// Triggers the normal attack of the archer.
        /// </summary>
    public override void NormalAttack(Action callback)
    {
        //Debug.Log("Archer Normal Attack");
        arrowHitTarget = false;
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);
        StartCoroutine(NormalAttackCR(callback));

    }

    /// <summary>
    /// The coroutine for the Archer's normal attack.
    /// </summary>
    /// <param name="callback">The function to call back to when the attack is completed.</param>
    protected override IEnumerator NormalAttackCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);


        ActionRange.Instance.ActionDeselected();
        StartCoroutine(LookToTarget());
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => IsTurning == false);

        int extraDamage = 0;

        AttackAnim();

        //yield return new WaitUntil(() => arrow.activ);
        yield return new WaitUntil(() => AnimationComplete);

        arrow.SetActive(true);

        if (hasTrueDamage && Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2, UnitToUpgrade.archer))
        {
            print("Activating golden trail.");
            extraDamage = AttackStat;
            if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade2, UnitToUpgrade.archer))
            {
                MovementStat = _baseMovement;
                AttackRange = _baseRange;
            }
            arrow.transform.GetChild(1).gameObject.SetActive(true);
            arrow.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
        }
        else
        {
            print("Activating standard trail.");
            arrow.transform.GetChild(0).gameObject.SetActive(true);
            arrow.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        }
        arrow.GetComponent<ProjectileMover>().SetTarget(CharacterSelector.Instance.SelectedTargetUnit);
        arrow.GetComponent<ProjectileMover>().EnableMove();

        yield return new WaitUntil(() => arrowHitTarget == true);

        //Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit is Enemy)
        {
            Enemy attackedEnemy = (Enemy)CharacterSelector.Instance.SelectedTargetUnit;

            int oldEnemyHealth = attackedEnemy.Health;

            //animatorController.SetTrigger("CastAttack");

            //yield return new WaitUntil(() => AnimationComplete);

            if (attackedEnemy.TakeDamage(AttackStat + extraDamage + (int)currentTile.TileBoost(TileEffect.Attack), hasTrueDamage))
            {
                if (!attackedEnemy.playersWhoAttacked.Contains(this)) attackedEnemy.playersWhoAttacked.Add(this);

                CombatSystem.Instance.KillUnit(attackedEnemy);
            }
            else if (!attackedEnemy.playersWhoAttacked.Contains(this) && attackedEnemy.Health < oldEnemyHealth)
            {
                attackedEnemy.playersWhoAttacked.Add(this);
            }

            ///If the attack doesn't kill the enemy, but does deal damage, and we have purchased the first upgrade
            ///for the basic attack, then we will apply a move speed debuff on the enemy hit.
            if (Upgrades.Instance.IsAbilityUnlocked(Abilities.normalAttackUpgrade1, UnitToUpgrade.archer)
                && attackedEnemy.Health < oldEnemyHealth)
            {
                StatusEffect effect = new StatusEffect(StatusEffect.StatusEffectType.MoveDown, 3, this, attackedEnemy);
                attackedEnemy.AddStatusEffect(effect);
                attackedEnemy.damagedThisTurn = false;
                attackedEnemy.MovementStat--;
            }
        }

        if (hasTrueDamage)
        {
            hasTrueDamage = false;
            DeactivateAbilityTwoParticle();
        }

        callback();
    }
    #endregion

    #region Ability One
    /// <summary>
    /// Triggers the archer's first ability which heals players.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        //Debug.Log("Archer Ability One");
        potionHitTarget = false;
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetPlayers);
        StartCoroutine(AbilityOneCR(callback));
    }

    /// <summary>
    /// Heals the player
    /// </summary>
    /// <param name="callback">The function to call back to when the ability is completed.</param>
    protected override IEnumerator AbilityOneCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        //_potion.SetActive(true);

        if (CharacterSelector.Instance.SelectedTargetUnit is Player player)
        {
            ActionRange.Instance.ActionDeselected();

            StartCoroutine(LookToTarget());
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => IsTurning == false);

            print("Done turning");

            Player target = player;

            AbilityOneAnim();

            if (ArcherAccController != null)
                ArcherAccController.SetTrigger("CastAbilityOne");

            yield return new WaitUntil(() => potionHitTarget == true);

            Vector3 targetPos = player.transform.position;

            potionSplash.transform.position = new Vector3(targetPos.x,
                                                          potionSplash.transform.position.y,
                                                          targetPos.z);

            potionSplash.Play();

            print("Potion hit target");
            target.Heal();
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => AnimationComplete);

            print("Archer heal anim complete.");

            StartAbilityOneCD();

            callback();
        }
    }

    public void LaunchPotion()
    {
        Player target = (Player)CharacterSelector.Instance.SelectedTargetUnit;

        potion.GetComponent<ProjectileMover>().SetTarget(target);
        potion.GetComponent<ProjectileMover>().EnableMove();
    }
    #endregion

    #region Ability Two
    /// <summary>
    /// Triggers the archer's second ability.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        //Debug.Log("Archer Ability Two");
        hasTrueDamage = true;
        ActionRange.Instance.ActionDeselected(false);

        if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade2, UnitToUpgrade.archer))
        {
            Debug.Log("Increasing attack and move range.");
            MovementStat += 2;
            AttackRange += 2;
            FindMovementRange();
            MapGrid.Instance.DrawBoarder(TileRange, ref CharacterSelector.Instance.boarderRenderer);
            FindActionRanges();
        }

        StartAbilityTwoCD();

        //animatorController.SetTrigger("CastEagleEye");

        ActivateAbilityTwoParticle();

        CombatSystem.Instance.SetBattleState(BattleState.Idle);
        CombatSystem.Instance.SetAbilityTwoButtonState(false);
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region Upgrade Functions
    protected override void AttackUpgradeOne()
    {
        Debug.Log("Enemy units will now have their move speed reduced when attack hits.");
    }

    protected override void AttackUpgradeTwo()
    {
        Debug.Log("Increases the accuracy of the player's basic attack.");
    }

    protected override void AbilityOneUpgradeOne()
    {
        Debug.Log("Heal now restores the target's health by 30% of their max.");
    }

    protected override void AbilityOneUpgradeTwo()
    {
        Debug.Log("Ability range is now 4 tiles.");
        AbilityOneRange = 4;
        FindActionRanges();
    }

    protected override void AbilityTwoUpgradeOne()
    {
        Debug.Log("Next attack will now deal double damage.");
    }

    protected override void AbilityTwoUpgradeTwo()
    {
        Debug.Log("While ability active move speed and attack range increased by 2.");
    }

    public override void ProcessUpgrade(Abilities abilityToUpgrade)
    {
        switch (abilityToUpgrade)
        {
            case Abilities.normalAttackUpgrade1:
                AttackUpgradeOne();
                break;

            case Abilities.normalAttackUpgrade2:
                AttackUpgradeTwo();
                break;

            case Abilities.ability1Upgrade1:
                AbilityOneUpgradeOne();
                break;

            case Abilities.ability1Upgrade2:
                AbilityOneUpgradeTwo();
                break;

            case Abilities.ability2Upgrade1:
                AbilityTwoUpgradeOne();
                break;

            case Abilities.ability2Upgrade2:
                AbilityTwoUpgradeTwo();
                break;

            default:
                break;
        }
    }
    #endregion
}
