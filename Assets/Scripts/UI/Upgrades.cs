using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


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
            _mageXp = value;
            if(_mageXp >= 100)
            {
                _mageXp -= 100;
                MagePoints++;
                ShowUpgradeNotification();
            }
            mageXpBar.value = _mageXp / 100f;
            _mageXpText.text = _mageXp + " / " + maxXP;
        }
    }

    public int KnightXp
    {
        get { return _knightXp; }
        set
        {
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

    public int ArcherXp
    {
        get { return _archerXp; }
        set
        {
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
    #endregion

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
        get{return _knightPoints;}
        set
        {
            _knightPoints = value;
            knightPointText.text = _knightPoints.ToString();
        }
    }

    public int ArcherPoints
    {
        get{return _archerPoints;}
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
        Text[] info = infoWindow.GetComponentsInChildren<Text>();
        infoTitleText = info[0];
        infoText = info[1];

        notificationText = notification.GetComponentInChildren<Text>();

        _mageXpText = mageXpBar.GetComponentInChildren<Text>();
        _knightXpText = knightXpBar.GetComponentInChildren<Text>();
        _archerXpText = archerXpBar.GetComponentInChildren<Text>();

        upgradeButtons = new List<UpgradeButton>(GetComponentsInChildren<UpgradeButton>(true));
        foreach(UpgradeButton upgradeButton in upgradeButtons)
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
        if(infoWindow.activeInHierarchy)
        {
            infoWindow.transform.position = Input.mousePosition;
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
    public void UnlockAbility(UnitToUpgrade unit, Abilities ability, Abilities abilityRequirement, int pointRequirement)
    {
        Debug.Log(ability);
        if(ability != Abilities.none && !IsAbilityUnlocked(ability, unit))
        {
            if(IsAbilityUnlocked(abilityRequirement, unit))
            {
                switch (unit)
                {
                    case UnitToUpgrade.mage:
                        if (MagePoints >= pointRequirement)
                        {
                            unlockedMageAbilities.Add(ability);
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
                DisplayPoints();
                SetButtonStates();
            }
            else
            {
                StartCoroutine(CantUnlockMessage("Must Unlock Ability First"));
            }
            
        }

    }

    
    public bool IsAbilityUnlocked(Abilities ability, UnitToUpgrade unit)
    {
        bool result = false;

        if(ability != Abilities.none)
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

    #endregion

    #region UI Functions

    public void DisplayPoints()
    {
        MagePoints = MagePoints;
        KnightPoints = KnightPoints;
        ArcherPoints = ArcherPoints;

        MageXp = MageXp;
        KnightXp = KnightXp;
        ArcherXp = ArcherXp;
    }

    public void SetButtonStates()
    {
        foreach(UpgradeButton upgradeButton in upgradeButtons)
        {
            if(IsAbilityUnlocked(upgradeButton.abilityToUnlock, upgradeButton.unitToUpgrade))
            {
                upgradeButton.GetComponent<Image>().color = Color.green;
            }
        }
    }

    

    public void SetInfoText(string info)
    {
        infoText.text = info;
    }
    
    public void SetTitleText(string title)
    {
        infoTitleText.text = title;
    }

    public void DisplayInfo()
    {
        
        infoWindow.SetActive(true);
    }

    public void HideInfo()
    {
        infoWindow.SetActive(false);
    }

    public void ToggleUpgradeMenu()
    {
        if(upgradeWindow.activeInHierarchy)
        {
            upgradeWindow.SetActive(false);
        }
        else
        {
            upgradeWindow.SetActive(true);
            SetButtonStates();
        }

        ClearUpgradeNotification();
    }

    public IEnumerator CantUnlockMessage(string message)
    {
        string temp = infoText.text;
        infoText.text = message;
        yield return new WaitForSeconds(1f);
        if(infoText.text == message)
        {
            infoText.text = temp;
        }
        
    }

    private void ShowUpgradeNotification()
    {
        notification.SetActive(true);
        notificationText.text = (MagePoints + KnightPoints + ArcherPoints) + "";
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
