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

    public enum ParticleToTrigger
    {
        Fireball
    }


    public void TriggerParticle(ParticleToTrigger particle)
    {
        //print(particle);

        transform.parent.GetComponentInChildren<Player>().AbilityOneParticle.gameObject.SetActive(true);
        transform.parent.GetComponentInChildren<Player>().ActivateAbilityOneParticle();
    }
}
