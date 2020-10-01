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
    public GameObject infoWindow;

    public Text magePointText;
    public Text knightPointText;
    public Text archerPointText;

    private Text infoText;


    public int mageSkillPoints;
    public int knightSkillPoints;
    public int archerSkillPoints;



    //public Abilities
    public List<UpgradeButton> upgradeButtons;


    public List<Abilities> unlockedMageAbilities;
    public List<Abilities> unlockedKnightAbilities;
    public List<Abilities> unlockedArcherAbilities;


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
        infoText = infoWindow.GetComponentInChildren<Text>();
        upgradeButtons = new List<UpgradeButton>(GetComponentsInChildren<UpgradeButton>());
        foreach(UpgradeButton upgradeButton in upgradeButtons)
        {
            upgradeButton.GetComponent<Button>().onClick.AddListener(() => UnlockAbility(upgradeButton.unitToUpgrade, upgradeButton.abilityToUnlock, upgradeButton.requiredAbility, upgradeButton.pointRequirement));
        }

        unlockedMageAbilities = new List<Abilities>();
        unlockedKnightAbilities = new List<Abilities>();
        unlockedArcherAbilities = new List<Abilities>();

        LoadUpgrades();
        DisplayPoints();
        SetButtonStates();
    }

    public void SaveUpgrades()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/upgrades_save.dat");

        UpgradeSave upgradeSave = new UpgradeSave();
        upgradeSave.unlockedMageAbilities = new List<Abilities>(unlockedMageAbilities);
        upgradeSave.unlockedKnightAbilities = new List<Abilities>(unlockedKnightAbilities);
        upgradeSave.unlockedArcherAbilities = new List<Abilities>(unlockedArcherAbilities);

        upgradeSave.mageSkillPoints = mageSkillPoints;
        upgradeSave.knightSkillPoints = knightSkillPoints;
        upgradeSave.archerSkillPoints = archerSkillPoints;

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

            mageSkillPoints = upgradeSave.mageSkillPoints;
            knightSkillPoints = upgradeSave.knightSkillPoints;
            archerSkillPoints = upgradeSave.archerSkillPoints;
        }


    }

    // Update is called once per frame
    void Update()
    {
        if(infoWindow.activeInHierarchy)
        {
            infoWindow.transform.position = Input.mousePosition;
        }
    }

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
                        if (mageSkillPoints >= pointRequirement)
                        {
                            unlockedMageAbilities.Add(ability);
                            mageSkillPoints -= pointRequirement;
                            Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");
                        }
                        else
                            StartCoroutine(CantUnlockMessage("Not Enough Points: Need " + pointRequirement));
                        break;
                    case UnitToUpgrade.knight:
                        if (knightSkillPoints >= pointRequirement)
                        {
                            unlockedKnightAbilities.Add(ability);
                            knightSkillPoints -= pointRequirement;
                            Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");
                        }
                        else
                            StartCoroutine(CantUnlockMessage("Not Enough Points: Need " + pointRequirement));
                        break;
                    case UnitToUpgrade.archer:
                        if (archerSkillPoints >= pointRequirement)
                        {
                            unlockedArcherAbilities.Add(ability);
                            archerSkillPoints -= pointRequirement;
                            Debug.Log("Unlocked " + unit + " " + ability + " for " + pointRequirement + " points");
                        }
                        else
                            StartCoroutine(CantUnlockMessage("Not Enough Points: Need " + pointRequirement));
                        break;
                    default:
                        break;
                }
                SaveUpgrades();
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

    public void DisplayPoints()
    {
        magePointText.text = mageSkillPoints.ToString();
        knightPointText.text = knightSkillPoints.ToString();
        archerPointText.text = archerSkillPoints.ToString();
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

    public void DisplayInfo()
    {
        
        infoWindow.SetActive(true);
    }

    public void HideInfo()
    {
        infoWindow.SetActive(false);
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
}



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
