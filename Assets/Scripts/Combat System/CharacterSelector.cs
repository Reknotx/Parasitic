﻿using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    // Update is called once per frame
    public static CharacterSelector Instance;

    [HideInInspector] public bool selectPlayer = true;
    [HideInInspector] public bool selectTarget = false;

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

                if (info.transform.gameObject.layer == 8 && selectPlayer)
                {
                    Debug.Log("Selected " + info.transform.gameObject.name);
                    //Debug.Log(info.transform.gameObject.layer);
                    CombatSystem.Instance.selectedPlayer = info.transform.gameObject.GetComponent<Player>();
                }
                else if ((info.transform.gameObject.layer == 8 || info.transform.gameObject.layer == 9) && selectTarget)
                {
                    Debug.Log("Selected " + info.transform.gameObject.name);
                    CombatSystem.Instance.target = info.transform.gameObject.GetComponent<Humanoid>();
                }

            }
        }
    }
}
