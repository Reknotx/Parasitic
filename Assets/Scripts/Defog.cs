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


    private int[] _defaultLayers;

    private void Awake()
    {
        _defaultLayers = new int[objectsToReveal.Length];
        for(int index = 0; index < objectsToReveal.Length; index++)
        {
            _defaultLayers[index] = objectsToReveal[index].layer;

            objectsToReveal[index].layer = 14;
            foreach (Transform child in objectsToReveal[index].transform)
            {
                child.gameObject.layer = 14;
            }
            
        }

        
    }


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
            for(int index = 0; index < objectsToReveal.Length; index++)
            {
                objectsToReveal[index].layer = _defaultLayers[index];
                foreach (Transform child in objectsToReveal[index].transform)
                {
                    child.gameObject.layer = _defaultLayers[index];
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
