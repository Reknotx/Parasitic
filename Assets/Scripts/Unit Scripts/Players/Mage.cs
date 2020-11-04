/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: Mage class file
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Player
{
    private bool killedUnitWhileEnchantActive = false;

    public Animator staffAndBookController;

    public override void Start()
    {
        if (CombatSystem.Instance)
        {
            print("Combat system exists");
        }
        else
        {
            print("Combat system gone");
        }
        healthBar = CombatSystem.Instance.mageHealthSlider;
        healthText = CombatSystem.Instance.mageHealthText;
        base.Start();
    }

    public override void Move(List<Tile> path, bool bypassRangeCheck = false)
    {
        staffAndBookController.SetBool("IsWalking", true);
        base.Move(path, bypassRangeCheck);
    }

    #region Normal Attack
    /// <summary>
    /// Mage's normal attack.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        //Debug.Log("Mage Normal Attack");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);
        StartCoroutine(NormalAttackCR(callback));
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        //Debug.Log("Select a target for the mage's normal attack.");

        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        StartCoroutine(LookToTarget());
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => IsTurning == false);
        LookToTarget();


        ActionRange.Instance.ActionDeselected();

        //Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit is Enemy)
        {
            int additionalDamage = 0;

            if (Upgrades.Instance.IsAbilityUnlocked(Abilities.normalAttackUpgrade1, UnitToUpgrade.mage))
            {
                float result = UnityEngine.Random.Range(0f, 1f);
                if (result <= 0.3f || true)
                {
                    Debug.Log("Applying additional damage.");
                    additionalDamage = Mathf.FloorToInt(AttackStat * 0.4f);
                }
            }

            int damageModifier = CheckForEffectOfType(StatusEffect.StatusEffectType.AttackUp) ? AttackStat / 2 : 0;

            if (damageModifier > 0 && killedUnitWhileEnchantActive)
            {
                ///Overrides the damage modifier if an enemy was killed while enchant
                ///was active.
                damageModifier = AttackStat;
            }

            damageModifier += additionalDamage;

            Enemy attackedEnemy = (Enemy)CharacterSelector.Instance.SelectedTargetUnit;

            int oldEnemyHealth = attackedEnemy.Health;

            AttackAnim();
            staffAndBookController.SetTrigger("CastAttack");

            yield return new WaitUntil(() => AnimationComplete);

            if (attackedEnemy.TakeDamage(AttackStat + damageModifier + (int)currentTile.TileBoost(TileEffect.Attack)))
            {
                if (!attackedEnemy.playersWhoAttacked.Contains(this)) attackedEnemy.playersWhoAttacked.Add(this);

                if (Upgrades.Instance.IsAbilityUnlocked(Abilities.normalAttackUpgrade2, UnitToUpgrade.mage))
                {
                    ///If the second upgrade is purchased and we have successfully killed the enemy, increase range by 1.
                    AttackRange++;
                }

                if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade2, UnitToUpgrade.mage))
                {
                    killedUnitWhileEnchantActive = true;
                }

                CombatSystem.Instance.KillUnit(attackedEnemy);
            }
            else if (!attackedEnemy.playersWhoAttacked.Contains(this) && attackedEnemy.Health < oldEnemyHealth)
            {
                attackedEnemy.playersWhoAttacked.Add(this);
            }
        }

        callback();
    }
    #endregion

    #region Ability One
    /// <summary>
    /// Mage's first ability. AOE fire blast.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        //Debug.Log("Mage Ability One");

        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);

        StartCoroutine(AbilityOneCR(callback));
    }

    /// <summary>
    /// AOE
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    protected override IEnumerator AbilityOneCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        int damageModifier = CheckForEffectOfType(StatusEffect.StatusEffectType.AttackUp) ? AttackStat / 2 : 0;
        Enemy focus = (Enemy)CharacterSelector.Instance.SelectedTargetUnit;
        bool[,] range = MapGrid.Instance.FindTilesInRange(focus.currentTile, 1, true, ActionShape.Square);
        Tile[,] tempGrid = MapGrid.Instance.grid;
        List<Enemy> enemies = new List<Enemy>();

        enemies.Add(focus);

        for (int i = 0; i < tempGrid.GetLength(0); i++)
        {
            for (int j = 0; j < tempGrid.GetLength(1); j++)
            {
                //Spot was not in range.
                if (!range[i, j]) continue;

                if (tempGrid[i, j].occupied && tempGrid[i, j].occupant is Enemy)
                {
                    if (!enemies.Contains((Enemy)(tempGrid[i, j].occupant)))
                        enemies.Add((Enemy)(tempGrid[i, j].occupant));
                }
            }
        }

        ///If the first upgrade to this ability is purchased base damage dealt will be half of mage's attack stat.
        int attackStatDivider = Upgrades.Instance.IsAbilityUnlocked(Abilities.ability1Upgrade1, UnitToUpgrade.mage) ? 2 : 3;

        int damageToDeal = (AttackStat / attackStatDivider) + damageModifier + (int)currentTile.TileBoost(TileEffect.Attack);

        List<Enemy> killList = new List<Enemy>();
        int oldEnemyHealth;

        AbilityOneAnim();
        staffAndBookController.SetTrigger("CastAbilityOne");

        yield return new WaitUntil(() => AnimationComplete);

        foreach (Enemy enemy in enemies)
        {
            oldEnemyHealth = enemy.Health;
            if (enemy.TakeDamage(damageToDeal))
            {
                if (!enemy.playersWhoAttacked.Contains(this)) enemy.playersWhoAttacked.Add(this);
                killList.Add(enemy);
            }

            if (enemy.Health < oldEnemyHealth)
            {
                if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability1Upgrade2, UnitToUpgrade.archer))
                {
                    ///Apply the debuff.
                    StatusEffect effect = new StatusEffect(StatusEffect.StatusEffectType.DefenseDown, 3, this, enemy);
                    enemy.DefenseStat -= 3;
                    enemy.AddStatusEffect(effect);
                }

                if (!enemy.playersWhoAttacked.Contains(this)) enemy.playersWhoAttacked.Add(this);
            }
        }

        foreach (Enemy enemy in killList)
        {
            CombatSystem.Instance.KillUnit(enemy);
            //Upgrades.Instance.MageXp += 50;
        }

        StartAbilityOneCD();

        callback();
    }
    #endregion

    #region Ability Two
    /// <summary>
    /// Mage's second ability. Damage boost.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        //Debug.Log("Mage Ability Two");

        //CreateAttackUpStatusEffect(this, this);

        StatusEffect attackUp = new StatusEffect(StatusEffect.StatusEffectType.AttackUp, 3, this, this);

        AddStatusEffect(attackUp);

        if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade1, UnitToUpgrade.mage))
        {
            StatusEffect moveUp = new StatusEffect(StatusEffect.StatusEffectType.MoveUp, 3, this, this);

            MovementStat += 2;

            FindMovementRange();
            MapGrid.Instance.DrawBoarder(TileRange, ref CharacterSelector.Instance.boarderRenderer);

            AddStatusEffect(moveUp);
        }

        AbilityTwoAnim();
        staffAndBookController.SetTrigger("CastAbilityTwo");

        ActionRange.Instance.ActionDeselected(false);

        CombatSystem.Instance.SetAbilityTwoButtonState(false);

        CombatSystem.Instance.SetBattleState(BattleState.Idle);

        killedUnitWhileEnchantActive = false;

        StartAbilityTwoCD();
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region Upgrade Functions
    protected override void AttackUpgradeOne()
    {
        Debug.Log("Attacks now have 30% chance to do 1.4x damage.");
    }

    protected override void AttackUpgradeTwo()
    {
        Debug.Log("Each kill with normal attack increases attack range by 1.");
    }

    protected override void AbilityOneUpgradeOne()
    {
        Debug.Log("Damage dealt by ability is now 1/2 of attack stat.");
    }

    protected override void AbilityOneUpgradeTwo()
    {
        Debug.Log("Enemy units damaged but not killed will have their defense reduced by 3 units.");
    }

    protected override void AbilityTwoUpgradeOne()
    {
        Debug.Log("Movement speed increased by 2 tiles while enchantment is active.");
    }

    protected override void AbilityTwoUpgradeTwo()
    {
        Debug.Log("When you get an enemy while enchantment is active, the damage will be raised to 2x for remaining durations.");
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
