/// AUTHOR: Jeremy Casada
/// DATE: 9/9/2020
/// 
/// Disables All objects in fogObjects while revealing all objects 
/// in objectsToReveal when player unit enters the attached trigger
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defog : MonoBehaviour
{

    [Header("FogObjects to be deactivated")]
    public GameObject[] fogObjects = new GameObject[1];

    [Header("Enemies and Other Objects to Reveal With this Trigger")]
    public GameObject[] objectsToReveal = new GameObject[1];

    [Header("Tag of Player, Make sure to Spell correctly")]
    public string playerTag = "";

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == playerTag)
        {
            HideFog();
            ChangeLayers();
        }
    }

    /// <summary>
    /// Changes objects in objectsToReveal to be on the Default Layer
    /// </summary>
    private void ChangeLayers()
    {
        if (objectsToReveal[0] != null)
        {
            foreach (GameObject objectToReveal in objectsToReveal)
            {
                objectToReveal.layer = 0;
                foreach (Transform child in objectToReveal.transform)
                {
                    child.gameObject.layer = 0;
                }
            }
        }
        
    }

    /// <summary>
    /// Hides objects in fogObjects
    /// </summary>
    private void HideFog()
    {
        if(fogObjects[0] != null)
        {
            foreach (GameObject fogObjects in fogObjects)
            {
                fogObjects.SetActive(false);
            }
        }
    }
}
