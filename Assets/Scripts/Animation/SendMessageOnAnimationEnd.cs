using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendMessageOnAnimationEnd : MonoBehaviour
{
    public void SendMessage()
    {
        transform.parent.GetComponentInChildren<Humanoid>().SetAnimationComplete();
    }
}
