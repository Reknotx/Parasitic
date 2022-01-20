using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public enum Condition
{
    KillEnemies,
    GetKeyItem,
    ReachArea
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
    public EnemyType typeToKill;
    public int defaultEnemiesKillRequirement = 0;
    public int allEnemiesInScene = 0;
    

    public Dropdown objectiveDropdown;
    public Text objectiveText;

    public bool orderedObjectives;
    [System.Serializable]
    public class Objective
    {
        public Condition condition = Condition.KillEnemies;
        public EnemyType typeToKill = EnemyType.Larva;
        public int enemiesKillRequirement = 0;
        public int numEnemiesInScene = 0;
        public string objectiveMessage = "Objective";
        public Item item = null;
        public ObjectiveZone objectiveZone = null;
        public bool complete;
    }

    public List<Objective> objectives = new List<Objective>();

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
       
        List<string> objectivesText = new List<string>();
        allEnemiesInScene = GetNumberOfEnemy(typeToKill);
        defaultEnemiesKillRequirement = allEnemiesInScene;
        bool allKillingObjectives = AllConditionsAreKilling();
        if(!allKillingObjectives)
        objectivesText.Add(GetKillMessage(defaultEnemiesKillRequirement, allEnemiesInScene, typeToKill));
        RectTransform itemRect = objectiveDropdown.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemRect.sizeDelta.y - Mathf.Clamp(13 * (objectives.Count - 1), 0, 30));

        if(objectives.Count > 0)
        {
            if (!allKillingObjectives)
            {
                objectivesText.Add("or");
            }

            for (int i = 0; i < objectives.Count; i++)
            {
                if(objectives[i].condition == Condition.KillEnemies)
                {
                    Objective tempObj = objectives[i];
                    tempObj.numEnemiesInScene = GetNumberOfEnemy(objectives[i].typeToKill);
                    objectives[i] = tempObj;
                    objectivesText.Add(GetKillMessage(objectives[i].enemiesKillRequirement, objectives[i].numEnemiesInScene, objectives[i].typeToKill));
                }
                else
                {
                    objectivesText.Add(objectives[i].objectiveMessage);
                }
            }
        }

        objectiveDropdown.AddOptions(objectivesText);

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


    string GetKillMessage(int killReq, int inScene, EnemyType enemy)
    {
        string killMessage;
        if (killReq == inScene)
        {
            if (enemy == EnemyType.AllTypes)
            {
                killMessage = "Kill all Enemies!";
            }
            else if (enemy == EnemyType.Larva)
            {
                killMessage = "Kill " + (killReq > 1 ? "all " : "the ") + (killReq > 1 ? "Larvae " : "Larva") + "!";
            }
            else
            {
                killMessage = "Kill " + (killReq > 1 ? "all " : "the ") + enemy + (killReq > 1 ? "s!" : "!");
            }
        }
        else
        {
            if (enemy == EnemyType.AllTypes)
            {
                killMessage = "Kill " + killReq + (killReq > 1 ? " Enemies!" : " Enemy!");
            }
            else if (enemy == EnemyType.Larva)
            {
                killMessage = "Kill " + killReq + (killReq > 1 ? "Larvae " : "Larva") + "!";
            }
            else
            {
                killMessage = "Kill " + killReq + " " + enemy + (killReq > 1 ? "s!" : "!");
            }

        }
        return killMessage;
    }

    public int GetNumberOfEnemy(EnemyType enemy)
    {
        int amount = 0;
        switch (enemy)
        {
            case EnemyType.AllTypes:
                amount = FindObjectsOfType<Enemy>().Length;
                break;
            case EnemyType.Larva:
                amount = FindObjectsOfType<Larva>().Length;
                break;
            case EnemyType.Shambler:
                amount = FindObjectsOfType<Shambler>().Length;
                break;
            case EnemyType.Spiker:
                amount = FindObjectsOfType<Spiker>().Length;
                break;
            case EnemyType.Charger:
                amount = FindObjectsOfType<Charger>().Length;
                break;
            case EnemyType.Brood:
                amount = FindObjectsOfType<Brood>().Length;
                break;
            case EnemyType.Hive:
                foreach (Hive hive in FindObjectsOfType<Hive>())
                {
                    if (hive is Brood) { }
                    else
                    {
                        amount++;
                    }
                }
                break;
            default:
                break;
        }
        return amount;
    }
    
    bool AllConditionsAreKilling()
    {
        bool allKilling = true;
        if(objectives.Count == 0)
        {
            return false;
        }
        foreach (Objective objective in objectives)
        {
            if(objective.condition != Condition.KillEnemies)
            {
                allKilling = false;
                break;
            }
        }
        return allKilling;
    }

    public bool CheckWinCondition(Condition condition)
    {
        if (condition == Condition.KillEnemies && CombatSystem.Instance.CheckKillCondition(EnemyType.AllTypes))
        {
            return true;
        }
        if(objectives.Count == 0)
        {
            return false;
        }
        bool objectivesComplete = true;
        foreach (Objective objective in objectives)
        {
            if(objective.condition == condition && !objective.complete)
            {
                switch (objective.condition)
                {
                    case Condition.KillEnemies:
                        objective.complete = CombatSystem.Instance.CheckKillCondition(objective.typeToKill);
                        break;
                    case Condition.GetKeyItem:
                        objective.complete = Inventory.Instance.InventoryContains(objective.item);
                        break;
                    case Condition.ReachArea:
                        objective.complete = CombatSystem.Instance.CheckAreaCondition(objective.objectiveZone);
                        break;
                    default:
                        break;
                }
            }
            if (!objective.complete)
            {
                objectivesComplete = false;
                if (orderedObjectives) break;
            }
        }
        return objectivesComplete;
    }
}




#if UNITY_EDITOR
[CustomEditor(typeof(WinConditions))]
public class WinConditionEditor : Editor
{
    
    WinConditions winCondition;
    bool showObjectives = false;
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

        //winCondition.condition = (Condition)EditorGUILayout.EnumPopup("Conditions", winCondition.condition);

        //winCondition.typeToKill = (EnemyType)EditorGUILayout.EnumPopup("Type of Enemy to Kill", winCondition.typeToKill);

        winCondition.orderedObjectives = (bool)EditorGUILayout.Toggle("Ordered Objectives",winCondition.orderedObjectives);
        //serializedObject.Update();
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("objectives"),);
        //serializedObject.ApplyModifiedProperties();

        //

        List<WinConditions.Objective> list = winCondition.objectives;

        showObjectives = EditorGUILayout.Foldout(showObjectives, "Objectives");
        if (showObjectives)
        {
            int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("size", list.Count));
            while (newCount < list.Count)
                list.RemoveAt(list.Count - 1);
            while (newCount > list.Count)
                list.Add(new WinConditions.Objective());
            EditorGUI.indentLevel++;
            //List<WinConditions.Objective> tmpList = new List<WinConditions.Objective>();
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Condition " + (i + 1), EditorStyles.boldLabel);
                //WinConditions.Objective tmp = new WinConditions.Objective();
                list[i].condition = (Condition)EditorGUILayout.EnumPopup("Condition", list[i].condition);


                if (list[i].condition == Condition.KillEnemies)
                {
                    list[i].typeToKill = (EnemyType)EditorGUILayout.EnumPopup("Type of Enemy to Kill", winCondition.objectives[i].typeToKill);

                    if (GUILayout.Button("Click to Set Requirement to All of Type: " + winCondition.objectives[i].typeToKill))
                    {
                        list[i].enemiesKillRequirement = winCondition.GetNumberOfEnemy(winCondition.objectives[i].typeToKill);

                    }
                    list[i].enemiesKillRequirement = EditorGUILayout.IntField("Required Kills for Type: " + winCondition.objectives[i].typeToKill, winCondition.objectives[i].enemiesKillRequirement);
                }
                else
                {
                    list[i].objectiveMessage = EditorGUILayout.TextField("Objective Message: ", winCondition.objectives[i].objectiveMessage);
                    if (list[i].condition == Condition.GetKeyItem)
                    {
                        list[i].item = (Item)EditorGUILayout.ObjectField("Item: ", winCondition.objectives[i].item, typeof(Item), true);
                    }
                    else if (list[i].condition == Condition.ReachArea)
                    {
                        list[i].objectiveZone = (ObjectiveZone)EditorGUILayout.ObjectField("Area: ", winCondition.objectives[i].objectiveZone, typeof(ObjectiveZone), true);
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
        
        //winCondition.objectives = new List<WinConditions.Objective>(tmpList);
        
        
       

        //
        //winCondition.objectives = (List<T>)EditorGUILayout.li
    }

    
}
#endif