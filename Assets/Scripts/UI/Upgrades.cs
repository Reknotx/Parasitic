/// AUTHOR: Jeremy Casada
/// DATE: 10/4/2020
///
///Controls the Upgrades / Skill Points of the Player Units
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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

    public GameObject upgradeWindow;
    public GameObject infoWindow;
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

    public Mage mage;
    public Warrior knight;
    public Archer archer;

    /// <summary>
    /// Experience System Vars
    /// </summary>
    #region EXP
    public Slider mageXpBar;
    public Slider knightXpBar;
    public Slider archerXpBar;

    public int maxXP = 100;

    private int _mageXp;
    private int _knightXp;
    private int _archerXp;

    private Text _mageXpText;
    private Text _knightXpText;
    private Text _archerXpText;


    public int MageXp
    {
        get { return _mageXp; }
        set
        {
            if(mage != null)
            {
                if(_mageXp != value) mage.ExpParticle.Play();
                _mageXp = value;
                if (_mageXp >= 100)
                {
                    _mageXp -= 100;
                    MagePoints++;
                    ShowUpgradeNotification();
                }
                mageXpBar.value = _mageXp / 100f;
                _mageXpText.text = _mageXp + " / " + maxXP;
            }
        }
    }

    public int KnightXp
    {
        get { return _knightXp; }
        set
        {
            if (knight != null)
            {
                if (_knightXp != value) knight.ExpParticle.Play();
                _knightXp = value;
                if (_knightXp >= 100)
                {
                    _knightXp -= 100;
                    KnightPoints++;
                    ShowUpgradeNotification();
                }
                knightXpBar.value = _knightXp / 100f;
                _knightXpText.text = _knightXp + " / " + maxXP;
            }
        }
    }

    public int ArcherXp
    {
        get { return _archerXp; }
        set
        {
            if (archer != null)
            {
                if (_archerXp != value) archer.ExpParticle.Play();
                _archerXp = value;
                if (_archerXp >= 100)
                {
                    _archerXp -= 100;
                    ArcherPoints++;
                    ShowUpgradeNotification();
                }
                archerXpBar.value = _archerXp / 100f;
                _archerXpText.text = _archerXp + " / " + maxXP;
            }
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
            magePointText.text = _magePoints.ToString();
        }
    }

    public int KnightPoints
    {
        get { return _knightPoints; }
        set
        {
            _knightPoints = value;
            knightPointText.text = _knightPoints.ToString();
        }
    }

    public int ArcherPoints
    {
        get { return _archerPoints; }
        set
        {
            _archerPoints = value;
            archerPointText.text = _archerPoints.ToString();
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

        //Get references to each upgrade button and add UnlockAbility Function to OnClick()
        upgradeButtons = new List<UpgradeButton>(GetComponentsInChildren<UpgradeButton>(true));
        foreach (UpgradeButton upgradeButton in upgradeButtons)
        {
            upgradeButton.GetComponent<Button>().onClick.AddListener(() => UnlockAbility(upgradeButton.unitToUpgrade, upgradeButton.abilityToUnlock, upgradeButton.requiredAbility, upgradeButton.pointRequirement));
        }

        unlockedMageAbilities = new List<Abilities>();
        unlockedKnightAbilities = new List<Abilities>();
        unlockedArcherAbilities = new List<Abilities>();


        DisplayPoints();
        SetButtonStates();
        ShowUpgradeNotification();
    }



    // Update is called once per frame
    void Update()
    {
        //Makes Info Window Follow Mouse While Active
        if (infoWindow.activeInHierarchy)
        {
            infoWindow.transform.position = Input.mousePosition;
        }

        if(Input.GetKeyDown(KeyCode.Tab) && !UI.Instance.PausedStatus)
        {
            ToggleUpgradeMenu();
        }
    }

    #region Saving/Loading (if needed in future)
    public void SaveUpgrades()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/upgrades_save.dat");

        UpgradeSave upgradeSave = new UpgradeSave();
        upgradeSave.unlockedMageAbilities = new List<Abilities>(unlockedMageAbilities);
        upgradeSave.unlockedKnightAbilities = new List<Abilities>(unlockedKnightAbilities);
        upgradeSave.unlockedArcherAbilities = new List<Abilities>(unlockedArcherAbilities);

        upgradeSave.mageSkillPoints = MagePoints;
        upgradeSave.knightSkillPoints = KnightPoints;
        upgradeSave.archerSkillPoints = ArcherPoints;

        bf.Serialize(file, upgradeSave);
        file.Close();

        Debug.Log("saved to " + Application.persistentDataPath + "/upgrades_save.dat");
    }

    public void LoadUpgrades()
    {
        if (File.Exists(Application.persistentDataPath + "/upgrades_save.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/upgrades_save.dat", FileMode.Open);
            UpgradeSave upgradeSave = (UpgradeSave)bf.Deserialize(file);
            file.Close();

            Debug.Log("loaded from " + Application.persistentDataPath + "/upgrades_save.dat");

            unlockedMageAbilities.AddRange(upgradeSave.unlockedMageAbilities);
            unlockedKnightAbilities.AddRange(upgradeSave.unlockedKnightAbilities);
            unlockedArcherAbilities.AddRange(upgradeSave.unlockedArcherAbilities);

            MagePoints = upgradeSave.mageSkillPoints;
            KnightPoints = upgradeSave.knightSkillPoints;
            ArcherPoints = upgradeSave.archerSkillPoints;
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
                            Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");
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
                            Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");
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
                            Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");
                        }
                        else
                            StartCoroutine(CantUnlockMessage("Not Enough Points: Need " + pointRequirement));
                        break;
                    default:
                        break;
                }
                //SaveUpgrades();
                SetButtonStates();
            }
            else
            {
                StartCoroutine(CantUnlockMessage("Must Unlock Ability First"));
            }

        }

    }

    /// <summary>
    /// Checks if Ability is Unlockable
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

        MageXp = MageXp;
        KnightXp = KnightXp;
        ArcherXp = ArcherXp;
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
        if (upgradeWindow.activeInHierarchy)
        {
            upgradeWindow.SetActive(false);
        }
        else
        {
            upgradeWindow.SetActive(true);
            DisplayPoints();
            SetButtonStates();
        }

        ClearUpgradeNotification();
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


    private void ShowUpgradeNotification()
    {

        if ((MagePoints + KnightPoints + ArcherPoints) > 0)
        {
            notification.SetActive(true);
            notificationText.text = (MagePoints + KnightPoints + ArcherPoints) + "";
        }

    }

    private void ClearUpgradeNotification()
    {
        notification.SetActive(false);
    }
    #endregion
}


//for loading and saving if we need it
[Serializable]
public class UpgradeSave
{
    public int mageSkillPoints;
    public int knightSkillPoints;
    public int archerSkillPoints;

    public List<Abilities> unlockedMageAbilities;
    public List<Abilities> unlockedKnightAbilities;
    public List<Abilities> unlockedArcherAbilities;

    // public UpgradeSave()
    // {
    //     unlockedMageAbilities = new List<Abilities>();
    //     unlockedKnightAbilities = new List<Abilities>();
    //     unlockedArcherAbilities = new List<Abilities>();
    //
    //     mageSkillPoints = 0;
    //     knightSkillPoints = 0;
    //     archerSkillPoints = 0;
    // }




}
