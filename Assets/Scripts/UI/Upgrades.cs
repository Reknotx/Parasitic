/// AUTHOR: Jeremy Casada
/// DATE: 10/4/2020
///
///Controls the Upgrades / Skill Points of the Player Units
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Enum used for list of unlocked abilities
/// </summary>
public enum Abilities
{
    none,
    normalAttackUpgrade1,
    normalAttackUpgrade2,
    ability1,
    ability1Upgrade1,
    ability1Upgrade2,
    ability2,
    ability2Upgrade1,
    ability2Upgrade2,
}

public class Upgrades : MonoBehaviour
{
    public static Upgrades Instance;

    [Header("Only Have This Checked For Scenes that are Part of A Multi Scene Level(But not the First Scene)"),
     Tooltip("Only Have This Checked For Scenes that are Part of A Multi Scene Level(But not the First Scene)")]
    public bool doLoadUpgrades = false;

    public GameObject upgradeWindow;
    public GameObject knightUpgrades;
    public GameObject mageUpgrades;
    public GameObject archerUpgrades;
    public GameObject infoWindow;
    public GameObject upgradesMenuToggle;
    public GameObject notification;

    public Text magePointText;
    public Text knightPointText;
    public Text archerPointText;

    private Text infoText;
    private Text infoTitleText;
    private Text notificationText;

    public List<UpgradeButton> upgradeButtons;


    public List<Abilities> unlockedMageAbilities;
    public List<Abilities> unlockedKnightAbilities;
    public List<Abilities> unlockedArcherAbilities;

    [Header("Sprites for Character Upgrade Background")]
    public Sprite mageBG;
    public Sprite knightBG;
    public Sprite archerBG;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    [HideInInspector]
    public Mage mage;
    [HideInInspector]
    public Warrior knight;
    [HideInInspector]
    public Archer archer;

    [HideInInspector]
    public int mageAttackRange;
    

    /// <summary>
    /// Experience System Vars
    /// </summary>
    #region EXP
    public Slider mageXpBar, mageXpBar2;
    public Slider knightXpBar, knightXpBar2;
    public Slider archerXpBar, archerXpBar2;

    public float maxXP = 100f;

    private int _mageXp;
    private int _knightXp;
    private int _archerXp;

    private Text _mageXpText, _mageXpText2;
    private Text _knightXpText, _knightXpText2;
    private Text _archerXpText, _archerXpText2;

    /// <summary>
    /// Mage EXP Property
    /// </summary>
    public int MageXp
    {
        get { return _mageXp; }
        set
        {
            if(mage != null)
            {
                if(_mageXp != value) mage.ExpParticle.Play();
                _mageXp = value;
                if (_mageXp >= maxXP)
                {
                    _mageXp -= (int)maxXP;
                    MagePoints++;
                }
                mageXpBar.value = _mageXp / maxXP;
                _mageXpText.text = _mageXp + "/" + maxXP;

                mageXpBar2.value = mageXpBar.value;
                _mageXpText2.text = _mageXpText.text;
            }
        }
    }

    /// <summary>
    /// Knight EXP Property
    /// </summary>
    public int KnightXp
    {
        get { return _knightXp; }
        set
        {
            if (knight != null)
            {
                if (_knightXp != value) knight.ExpParticle.Play();
                _knightXp = value;
                if (_knightXp >= maxXP)
                {
                    _knightXp -= (int)maxXP;
                    KnightPoints++;
                }
                knightXpBar.value = _knightXp / maxXP;
                _knightXpText.text = _knightXp + "/" + maxXP;

                knightXpBar2.value = knightXpBar.value;
                _knightXpText2.text = _knightXpText.text;
            }
        }
    }

    /// <summary>
    /// Archer EXP Property
    /// </summary>
    public int ArcherXp
    {
        get { return _archerXp; }
        set
        {
            if (archer != null)
            {
                if (_archerXp != value) archer.ExpParticle.Play();
                _archerXp = value;
                if (_archerXp >= maxXP)
                {
                    _archerXp -= (int)maxXP;
                    ArcherPoints++;
                }
                archerXpBar.value = _archerXp / maxXP;
                _archerXpText.text = _archerXp + "/" + maxXP;

                archerXpBar2.value = archerXpBar.value;
                _archerXpText2.text = _archerXpText.text;
            }
        }
    }

    /// <summary>
    /// Gets xp amount from input Enemy and splits it between the players who damaged that enemy
    /// </summary>
    /// <param name="enemy">enemy to get xp from</param>
    public void SplitExp(Enemy enemy)
    {
        int splitXp = enemy.XpDrop / enemy.playersWhoAttacked.Count;
        foreach(Player p in enemy.playersWhoAttacked)
        {
            if (p is Mage) MageXp += splitXp;
            else if (p is Warrior) KnightXp += splitXp;
            else if (p is Archer) ArcherXp += splitXp;
            Debug.Log(p.name + " Received " + splitXp + " EXP for helping eliminate " + enemy.name);
        }
    }
    #endregion

    /// <summary>
    /// Skill Point System Vars
    /// </summary>
    #region Skill Points
    public int _magePoints;
    public int _knightPoints;
    public int _archerPoints;


    public int MagePoints
    {
        get { return _magePoints; }
        set
        {
            _magePoints = value;
            magePointText.text = _magePoints + " point";
            if (_magePoints > 1 || _magePoints < 1)
                magePointText.text += "s";

            SetLockStates();
        }
    }

    public int KnightPoints
    {
        get { return _knightPoints; }
        set
        {
            _knightPoints = value;
            knightPointText.text = _knightPoints + " point";
            if (_knightPoints > 1 || _knightPoints < 1)
                knightPointText.text += "s";

            SetLockStates();

        }
    }

    public int ArcherPoints
    {
        get { return _archerPoints; }
        set
        {
            _archerPoints = value;
            archerPointText.text = _archerPoints + " point";
            if (_archerPoints > 1 || _archerPoints < 1)
                archerPointText.text += "s";

            SetLockStates();
        }
    }
    #endregion



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        mage = Mage.Instance;
        knight = Warrior.Instance;
        archer = Archer.Instance;
        //Gets Components of Info Hover Window
        Text[] info = infoWindow.GetComponentsInChildren<Text>();
        infoTitleText = info[0];
        infoText = info[1];

        //Gets Text for Notification Bubble
        notificationText = notification.GetComponentInChildren<Text>();

        //Get Text Components for each of the XP Bars
        _mageXpText = mageXpBar.GetComponentInChildren<Text>();
        _knightXpText = knightXpBar.GetComponentInChildren<Text>();
        _archerXpText = archerXpBar.GetComponentInChildren<Text>();

        mageXpBar2 = CombatSystem.Instance.mageXpSlider;
        knightXpBar2 = CombatSystem.Instance.knightXpSlider;
        archerXpBar2 = CombatSystem.Instance.archerXpSlider;

        _mageXpText2 = CombatSystem.Instance.mageXpText;
        _knightXpText2 = CombatSystem.Instance.knightXpText;
        _archerXpText2 = CombatSystem.Instance.archerXpText;

        //Get references to each upgrade button and add UnlockAbility Function to OnClick()
        upgradeButtons = new List<UpgradeButton>(GetComponentsInChildren<UpgradeButton>(true));
        foreach (UpgradeButton upgradeButton in upgradeButtons)
        {
            upgradeButton.GetComponent<Button>().onClick.AddListener(() => UnlockAbility(upgradeButton.unitToUpgrade, upgradeButton.abilityToUnlock, upgradeButton.requiredAbility, upgradeButton.pointRequirement));
        }

        unlockedMageAbilities = new List<Abilities>();
        unlockedKnightAbilities = new List<Abilities>();
        unlockedArcherAbilities = new List<Abilities>();

        if(doLoadUpgrades)
        {
            LoadUpgrades();
        }
        else
        {
            mageAttackRange = mage.AttackRange;
        }

        DisplayPoints();
        SetButtonStates();
        SetLockStates();
    }



    // Update is called once per frame
    void Update()
    {
        //Makes Info Window Follow Mouse While Active
        if (infoWindow.activeInHierarchy)
        {
            infoWindow.transform.position = Input.mousePosition;
        }

        if(Input.GetKeyDown(KeyCode.Tab) && !UI.Instance.PausedStatus && CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            ToggleUpgradeMenu();
        }
    }

    #region Saving/Loading 
    public void SaveUpgrades()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/upgrades_save.dat");

        UpgradeSave upgradeSave = new UpgradeSave();

        upgradeSave.mageAttackRange = mageAttackRange;
        

        upgradeSave.unlockedMageAbilities = new List<Abilities>(unlockedMageAbilities);
        upgradeSave.unlockedKnightAbilities = new List<Abilities>(unlockedKnightAbilities);
        upgradeSave.unlockedArcherAbilities = new List<Abilities>(unlockedArcherAbilities);

        upgradeSave.mageSkillPoints = MagePoints;
        upgradeSave.knightSkillPoints = KnightPoints;
        upgradeSave.archerSkillPoints = ArcherPoints;

        upgradeSave.mageXp = MageXp;
        upgradeSave.knightXp = KnightXp;
        upgradeSave.archerXp = ArcherXp;

        bf.Serialize(file, upgradeSave);
        file.Close();

        //Debug.Log("saved to " + Application.persistentDataPath + "/upgrades_save.dat");
    }

    public void LoadUpgrades()
    {
        if (File.Exists(Application.persistentDataPath + "/upgrades_save.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/upgrades_save.dat", FileMode.Open);
            UpgradeSave upgradeSave = (UpgradeSave)bf.Deserialize(file);
            file.Close();
           
            //Debug.Log("loaded from " + Application.persistentDataPath + "/upgrades_save.dat");
            //Debug.Log("Mage Attack: " + upgradeSave.mageAttackRange);
           
            
           
            foreach (Abilities ability in upgradeSave.unlockedMageAbilities)
            {
                UnlockAbility(UnitToUpgrade.mage, ability, Abilities.none, 0);
            }
           
            foreach (Abilities ability in upgradeSave.unlockedKnightAbilities)
            {
                UnlockAbility(UnitToUpgrade.knight, ability, Abilities.none, 0);
            }
           
            foreach (Abilities ability in upgradeSave.unlockedArcherAbilities)
            {
                UnlockAbility(UnitToUpgrade.archer, ability, Abilities.none, 0);
            }

            mage.AttackRange = upgradeSave.mageAttackRange;

            

            MagePoints = upgradeSave.mageSkillPoints;
            KnightPoints = upgradeSave.knightSkillPoints;
            ArcherPoints = upgradeSave.archerSkillPoints;

            MageXp = upgradeSave.mageXp;
            KnightXp = upgradeSave.knightXp;
            ArcherXp = upgradeSave.archerXp;
        }


    }
    #endregion


    #region Ability Unlocking

    /// <summary>
    /// Attemps to Unlock Ability Based on input requirements
    /// </summary>
    /// <param name="unit"> Enumerator Describing Unit Type</param>
    /// <param name="ability">Enumerator Describing Ability Type to Attempt Unlock</param>
    /// <param name="abilityRequirement">Required Ability Type to Unlock Desired "ability"</param>
    /// <param name="pointRequirement">Required Number of points to unlock "ability"</param>
    public void UnlockAbility(UnitToUpgrade unit, Abilities ability, Abilities abilityRequirement, int pointRequirement)
    {
        Debug.Log(ability);
        if (ability != Abilities.none && !IsAbilityUnlocked(ability, unit))
        {
            if (IsAbilityUnlocked(abilityRequirement, unit))
            {
                switch (unit)
                {
                    case UnitToUpgrade.mage:
                        if (MagePoints >= pointRequirement)
                        {
                            unlockedMageAbilities.Add(ability);
                            SetAbilityButtonState(ability, unit);
                            MagePoints -= pointRequirement;
                            //Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");

                            if (ability != Abilities.ability1 || ability != Abilities.ability2)
                            {
                                mage.ProcessUpgrade(ability);
                            }
                        }
                        else
                            StartCoroutine(CantUnlockMessage("Not Enough Points: Need " + pointRequirement));
                        break;
                    case UnitToUpgrade.knight:
                        if (KnightPoints >= pointRequirement)
                        {
                            unlockedKnightAbilities.Add(ability);
                            SetAbilityButtonState(ability, unit);
                            KnightPoints -= pointRequirement;
                            //Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");

                            if (ability != Abilities.ability1 || ability != Abilities.ability2)
                            {
                               knight.ProcessUpgrade(ability);
                            }
                        }
                        else
                            StartCoroutine(CantUnlockMessage("Not Enough Points: Need " + pointRequirement));
                        break;
                    case UnitToUpgrade.archer:
                        if (ArcherPoints >= pointRequirement)
                        {
                            unlockedArcherAbilities.Add(ability);
                            SetAbilityButtonState(ability, unit);
                            ArcherPoints -= pointRequirement;
                            //Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");

                            if (ability != Abilities.ability1 || ability != Abilities.ability2)
                            {
                                archer.ProcessUpgrade(ability);
                            }
                        }
                        else
                            StartCoroutine(CantUnlockMessage("Not Enough Points: Need " + pointRequirement));
                        break;
                    default:
                        break;
                }
                //SaveUpgrades();
                SetButtonStates();
                SetLockStates();
            }
            else
            {
                StartCoroutine(CantUnlockMessage("Must Unlock Ability First"));
            }

        }

    }

    /// <summary>
    /// Checks if Ability is Unlocked
    /// </summary>
    /// <param name="ability"> Enumerator Describing Ability to Check for</param>
    /// <param name="unit"> Enumerator Describing Unit Type </param>
    /// <returns></returns>
    public bool IsAbilityUnlocked(Abilities ability, UnitToUpgrade unit)
    {
        bool result = false;

        if (ability != Abilities.none)
        {
            switch (unit)
            {
                case UnitToUpgrade.mage:
                    result = unlockedMageAbilities.Contains(ability);
                    break;
                case UnitToUpgrade.knight:
                    result = unlockedKnightAbilities.Contains(ability);
                    break;
                case UnitToUpgrade.archer:
                    result = unlockedArcherAbilities.Contains(ability);
                    break;
                default:
                    break;

            }
        }
        else
        {
            result = true;
        }

        return result;
    }

    /// <summary>
    /// Looks to the character selected and evaluates the type of character we
    /// have selected.
    /// </summary>
    /// <returns>A UnitToUpgrade enum value representing the player we have selected</returns>
    /// Author: Chase O'Connor
    /// Date: 10/5/2020
    public UnitToUpgrade GetUnitType()
    {
        UnitToUpgrade unitType = UnitToUpgrade.knight;

        if (CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            if (CharacterSelector.Instance.SelectedPlayerUnit is Warrior)
            {
                unitType = UnitToUpgrade.knight;
            }
            else if (CharacterSelector.Instance.SelectedPlayerUnit is Archer)
            {
                unitType = UnitToUpgrade.archer;
            }
            else
            {
                unitType = UnitToUpgrade.mage;
            }
        }

        return unitType;
    }

    /// <summary>
    /// Sets states of ability button based on input ability
    /// </summary>
    /// <param name="ability">ability to set</param>
    private void SetAbilityButtonState(Abilities ability, UnitToUpgrade unitType)
    {
        if(CharacterSelector.Instance.SelectedPlayerUnit != null && GetUnitType() == unitType)
        {
            if (ability == Abilities.ability1)
            {

                CombatSystem.Instance.SetAbilityOneButtonState(true);

            }
            else if (ability == Abilities.ability2)
            {
                CombatSystem.Instance.SetAbilityTwoButtonState(true);
            }
        }
    }

    #endregion



    #region UI Functions

    /// <summary>
    /// Used to Set UI for EXP and Points in Start
    /// </summary>
    public void DisplayPoints()
    {
        MagePoints = MagePoints;
        KnightPoints = KnightPoints;
        ArcherPoints = ArcherPoints;

        MageXp = _mageXp;
        KnightXp = _knightXp;
        ArcherXp = _archerXp;
    }

    /// <summary>
    /// Sets Color of Upgrade Button to Green if already Unlocked
    /// </summary>
    public void SetButtonStates()
    {
        foreach (UpgradeButton upgradeButton in upgradeButtons)
        {
            if (IsAbilityUnlocked(upgradeButton.abilityToUnlock, upgradeButton.unitToUpgrade))
            {
                upgradeButton.GetComponent<Image>().color = Color.green;
            }
        }
    }

    public void SetLockStates()
    {
        foreach (UpgradeButton upgradeButton in upgradeButtons)
        {
            if (IsAbilityUnlocked(upgradeButton.abilityToUnlock, upgradeButton.unitToUpgrade))
            {
                upgradeButton.lockImage.gameObject.SetActive(false);
            }
            else if (upgradeButton.abilityToUnlock != Abilities.none && IsAbilityUnlocked(upgradeButton.requiredAbility, upgradeButton.unitToUpgrade) && doHaveEnoughPoints(upgradeButton))
            {
                upgradeButton.lockImage.sprite = unlockedSprite;
            }
            else
            {
                upgradeButton.lockImage.sprite = lockedSprite;
            }
            
        }
    }

    public bool doHaveEnoughPoints(UpgradeButton upgradeButton)
    {
        bool result = false;
        switch (upgradeButton.unitToUpgrade)
        {
            case UnitToUpgrade.mage:
                if(MagePoints >= upgradeButton.pointRequirement)
                {
                    result = true;
                }
                break;
            case UnitToUpgrade.knight:
                if (KnightPoints >= upgradeButton.pointRequirement)
                {
                    result = true;
                }
                break;
            case UnitToUpgrade.archer:
                if (ArcherPoints >= upgradeButton.pointRequirement)
                {
                    result = true;
                }
                break;
            case UnitToUpgrade.none:
                break;
            default:
                break;
        }
        return result;
    }


    /// <summary>
    /// Sets Description of Info window
    /// </summary>
    /// <param name="info"></param>
    public void SetInfoText(string info)
    {
        infoText.text = info;
    }

    /// <summary>
    /// Sets Title of Info Window
    /// </summary>
    /// <param name="title"></param>
    public void SetTitleText(string title)
    {
        infoTitleText.text = title;
    }

    /// <summary>
    /// Activates Info Window
    /// </summary>
    public void DisplayInfo()
    {
        infoWindow.SetActive(true);
    }

    /// <summary>
    /// Hides Info Window
    /// </summary>
    public void HideInfo()
    {
        infoWindow.SetActive(false);
    }

    /// <summary>
    /// Toggles the Upgrade Menu and Clears Notification
    /// </summary>
    public void ToggleUpgradeMenu()
    {
        Player temp = CharacterSelector.Instance.SelectedPlayerUnit;
        if (upgradeWindow.activeInHierarchy)
        {
            knightUpgrades.SetActive(false);
            mageUpgrades.SetActive(false);
            archerUpgrades.SetActive(false);
            upgradeWindow.SetActive(false);
            HideInfo();
        }
        else if (temp.State != HumanoidState.Moving && CombatSystem.Instance.state != BattleState.Lost && CombatSystem.Instance.state != BattleState.Won)
        {
            if(temp is Warrior)
            {
                knightUpgrades.SetActive(true);
                upgradeWindow.GetComponent<Image>().sprite = knightBG;
            }
            else if (temp is Mage)
            {
                mageUpgrades.SetActive(true);
                upgradeWindow.GetComponent<Image>().sprite = mageBG;
            }
            else if (temp is Archer)
            {
                archerUpgrades.SetActive(true);
                upgradeWindow.GetComponent<Image>().sprite = archerBG;
            }

            
            upgradeWindow.SetActive(true);
            DisplayPoints();
            SetButtonStates();
            CombatSystem.Instance.Cancel(false);
        }

        
    }

    /// <summary>
    /// Used to Temporarily Display Message in info window
    /// </summary>
    /// <param name="message">message to display</param>
    /// <returns></returns>
    public IEnumerator CantUnlockMessage(string message)
    {
        string temp = infoText.text;
        infoText.text = message;
        yield return new WaitForSeconds(1f);
        if (infoText.text == message)
        {
            infoText.text = temp;
        }

    }



    #endregion
}



[Serializable]
public class UpgradeSave
{
    public int mageAttackRange;


    public int mageSkillPoints;
    public int knightSkillPoints;
    public int archerSkillPoints;

    public int mageXp;
    public int knightXp;
    public int archerXp;

    public List<Abilities> unlockedMageAbilities;
    public List<Abilities> unlockedKnightAbilities;
    public List<Abilities> unlockedArcherAbilities;


}
