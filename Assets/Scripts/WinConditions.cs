using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public enum Condition
{
    KillEnemies,
    KillEnemiesOrGetKey
}
public class WinConditions : MonoBehaviour
{
    public static WinConditions Instance;
    public Condition condition;


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

        List<string> objectives = new List<string>();


        if (enemiesKillRequirement == numEnemiesInScene)
        {
            objectives.Add("Kill all Enemies!");
        }
        else
        {
            if(enemiesKillRequirement > 1)
            {
                objectives.Add("Kill " + enemiesKillRequirement + " Enemies");
            }
            else
            {
                objectives.Add("Kill " + enemiesKillRequirement + " Enemy");
            }
            
        }


        if((int)condition == 1)
        {
            objectives.Add("or");
            objectives.Add(keyObjectiveMessage);
        }

        objectiveDropdown.AddOptions(objectives);


        switch (condition)
        {
            case Condition.KillEnemies:
                objectiveText.text = "Objective";
                break;
            case Condition.KillEnemiesOrGetKey:
                objectiveText.text = "Objectives";
                break;
            default:
                break;
        }
    }
}

[CustomEditor(typeof(WinConditions))]
public class WinConditionEditor : Editor
{
    
    WinConditions winCondition;
    int[] choices = { 0, 1 }; 

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

        winCondition.objectiveDropdown = (Dropdown)EditorGUILayout.ObjectField("Objectives Drop Down", winCondition.objectiveDropdown, typeof(Dropdown), true);
        winCondition.objectiveText = (Text)EditorGUILayout.ObjectField("Objective Text", winCondition.objectiveText, typeof(Text), true);

        winCondition.condition = (Condition)EditorGUILayout.EnumPopup("Conditions", winCondition.condition);



        winCondition.enemiesKillRequirement = EditorGUILayout.IntField("Required Enemy Kill Count", winCondition.enemiesKillRequirement);
        if (GUILayout.Button("Click to Set Requirement to every Enemy"))
        {
            winCondition.enemiesKillRequirement = FindObjectsOfType<Enemy>().Length;
        }

        if((int)winCondition.condition == 1)
        {
            winCondition.keyObjectiveMessage = EditorGUILayout.TextField("Message for Key Condition", winCondition.keyObjectiveMessage);
        }




    }

    
}