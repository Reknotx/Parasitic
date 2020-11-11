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

        switch (particle)
        {
            case ParticleToTrigger.Fireball:
                Mage.Instance.AbilityOneParticle.gameObject.SetActive(true);
                Mage.Instance.ActivateAbilityOneParticle();
                break;
            default:
                break;
        }
        //transform.parent.GetComponentInChildren<Player>().AbilityOneParticle.gameObject.SetActive(true);
        //transform.parent.GetComponentInChildren<Player>().ActivateAbilityOneParticle();
    }

    public void ThrowParticle(ParticleToTrigger particle)
    {
        if (particle == ParticleToTrigger.Fireball)
        {
            Mage.Instance.AbilityOneParticle.GetComponent<ProjectileMover>().EnableMove();
        }
    }
}
