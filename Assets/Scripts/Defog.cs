using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defog : MonoBehaviour
{
    [Header("Group of Clouds to be deactivated")]
    public GameObject[] cloudGroups = new GameObject[1];

    [Header("Enemies and Other Objects to Reveal With this Trigger")]
    public GameObject[] objectsToReveal = new GameObject[1];


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "playerUnit")
        {
            HideClouds();
            ChangeLayers();
        }
    }

    private void ChangeLayers()
    {
        foreach( GameObject objectToReveal in objectsToReveal)
        {
            objectToReveal.layer = 0;
        }
    }

    private void HideClouds()
    {
        foreach(GameObject cloudGroup in cloudGroups)
        {
            cloudGroup.SetActive(false);
        }
    }
}
