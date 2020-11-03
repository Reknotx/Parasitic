using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendMessageOnEvent : MonoBehaviour
{
    public void AnimationStart()
    {
        transform.parent.GetComponentInChildren<Humanoid>().SetAnimationComplete(false);
    }

    public void AnimationEnd()
    {
        transform.parent.GetComponentInChildren<Humanoid>().SetAnimationComplete(true);
    }
}
