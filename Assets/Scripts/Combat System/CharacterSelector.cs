using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    // Update is called once per frame
    public static CharacterSelector Instance;

    //[HideInInspector] public bool selectPlayer = true;
    //[HideInInspector] public bool selectTarget = false;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit info = new RaycastHit();

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out info);

            if (hit)
            {
                Debug.Log("Selected " + info.transform.gameObject.name);

                if (info.transform.gameObject.layer == 8 && CombatSystem.Instance.state == BattleState.Player)
                {
                    CombatSystem.Instance.SetPlayer(info.transform.gameObject.GetComponent<Player>());
                }
                else if (CombatSystem.Instance.state == BattleState.Targetting)
                {
                    CombatSystem.Instance.SetTarget(info.transform.gameObject.GetComponent<Humanoid>());
                }
            }
        }
    }
}
