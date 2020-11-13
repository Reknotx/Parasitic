using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public enum Condition
{
    KillEnemies,
    KillEnemiesOrGetKeyItem
}

public enum EnemyType
{
    AllTypes,
    Larva,
    Shambler,
    Spiker,
    Charger,
    Brood,
    Hive
}

public class WinConditions : MonoBehaviour
{
    public static WinConditions Instance;
    public Condition condition;
    public EnemyType typeToKill;


    public int enemiesKillRequirement = 0;
    public int numEnemiesInScene = 0;
    public string keyObjectiveMessage;

    public Dropdown objectiveDropdown;
    public Text objectiveText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        numEnemiesInScene = FindObjectsOfType<Enemy>().Length;
        switch (typeToKill)
        {
            case EnemyType.AllTypes:
                numEnemiesInScene = FindObjectsOfType<Enemy>().Length;
                break;
            case EnemyType.Larva:
                numEnemiesInScene = FindObjectsOfType<Larva>().Length;
                break;
            case EnemyType.Shambler:
                numEnemiesInScene = FindObjectsOfType<Shambler>().Length;
                break;
            case EnemyType.Spiker:
                numEnemiesInScene = FindObjectsOfType<Spiker>().Length;
                break;
            case EnemyType.Charger:
                numEnemiesInScene = FindObjectsOfType<Charger>().Length;
                break;
            case EnemyType.Brood:
                numEnemiesInScene = FindObjectsOfType<Brood>().Length;
                break;
            case EnemyType.Hive:
                numEnemiesInScene = FindObjectsOfType<Hive>().Length;
                break;
            default:
                break;
        }

        List<string> objectives = new List<string>();


        if (enemiesKillRequirement == numEnemiesInScene)
        {
            if(typeToKill == EnemyType.AllTypes)
            {
                objectives.Add("Kill all Enemies!");
            }
            else
            {
                if(typeToKill == EnemyType.Larva || typeToKill == EnemyType.Brood)
                {
                    objectives.Add("Kill all " + typeToKill + "!");
                }
                else
                {
                    objectives.Add("Kill all " + typeToKill + "s!");
                }
            }
            
        }
        else
        {
            if(enemiesKillRequirement > 1)
            {
                if(typeToKill == EnemyType.Larva || typeToKill == EnemyType.Brood)
                {
                    objectives.Add("Kill " + enemiesKillRequirement + " " + typeToKill + "!");
                }
                else
                {
                    objectives.Add("Kill " + enemiesKillRequirement + " " + typeToKill + "s!");
                }
                
            }
            else
            {
                if (typeToKill == EnemyType.AllTypes)
                {
                    objectives.Add("Kill " + enemiesKillRequirement + " Enemy!");
                }
                else
                {
                    objectives.Add("Kill " + enemiesKillRequirement + " " + typeToKill + "!");
                }
            }
            
        }


        if((int)condition == 1)
        {
            objectives.Add("or");
            objectives.Add(keyObjectiveMessage);
        }

        objectiveDropdown.AddOptions(objectives);


       // switch (condition)
       // {
       //     case Condition.KillEnemies:
       //         objectiveText.text = "Objective";
       //         break;
       //     case Condition.KillEnemiesOrGetKeyItem:
       //         objectiveText.text = "Objectives";
       //         break;
       //     default:
       //         break;
       // }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WinConditions))]
public class WinConditionEditor : Editor
{
    
    WinConditions winCondition;

    //bool ShowEnumOption(System.Enum Enum)
    //{
    //    Condition cond = (Condition)Enum;
    //
    //    bool result = true;
    //    
    //    if(cond == winCondition.condition)
    //    {
    //        result = false;
    //    }
    //
    //    return result;
    //}
    
    

    void OnEnable()
    {
        winCondition = (WinConditions)target;
    }

    public override void OnInspectorGUI()
    {
        //System.Func<System.Enum, bool> showEnumOption = ShowEnumOption;

        winCondition.objectiveDropdown = (Dropdown)EditorGUILayout.ObjectField("Objectives Drop Down(dont touch)", winCondition.objectiveDropdown, typeof(Dropdown), true);
        //winCondition.objectiveText = (Text)EditorGUILayout.ObjectField("Objective Text(dont touch)", winCondition.objectiveText, typeof(Text), true);

        winCondition.condition = (Condition)EditorGUILayout.EnumPopup("Conditions", winCondition.condition);

        winCondition.typeToKill = (EnemyType)EditorGUILayout.EnumPopup("Type of Enemy to Kill", winCondition.typeToKill);



        
        if (GUILayout.Button("Click to Set Requirement to All of Type: " + winCondition.typeToKill))
        {
            switch (winCondition.typeToKill)
            {
                case EnemyType.AllTypes:
                    winCondition.enemiesKillRequirement = FindObjectsOfType<Enemy>().Length;
                    break;
                case EnemyType.Larva:
                    winCondition.enemiesKillRequirement = FindObjectsOfType<Larva>().Length;
                    break;
                case EnemyType.Shambler:
                    winCondition.enemiesKillRequirement = FindObjectsOfType<Shambler>().Length;
                    break;
                case EnemyType.Spiker:
                    winCondition.enemiesKillRequirement = FindObjectsOfType<Spiker>().Length;
                    break;
                case EnemyType.Charger:
                    winCondition.enemiesKillRequirement = FindObjectsOfType<Charger>().Length;
                    break;
                case EnemyType.Brood:
                    winCondition.enemiesKillRequirement = FindObjectsOfType<Brood>().Length;
                    break;
                case EnemyType.Hive:
                    winCondition.enemiesKillRequirement = 0;
                    foreach(Hive hive in FindObjectsOfType<Hive>())
                    {
                        if (hive is Brood) { }
                        else
                        {
                            winCondition.enemiesKillRequirement++;
                        }
                    }
                    break;
                default:
                    break;
            }
            
        }
        winCondition.enemiesKillRequirement = EditorGUILayout.IntField("Required Kills for Type: " + winCondition.typeToKill, winCondition.enemiesKillRequirement);

        if ((int)winCondition.condition == 1)
        {
            winCondition.keyObjectiveMessage = EditorGUILayout.TextField("Message for Key Condition", winCondition.keyObjectiveMessage);
        }



    }

    
}
#endif