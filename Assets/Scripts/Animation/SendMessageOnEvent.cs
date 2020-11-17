/*
 * Author: Chase O'Connor
 * Date: 10/27/2020
 * 
 * Brief: Simple little script to be attached to the rigs for the characters.
 * Allows for messages to be sent to the humanoid script to allow for the coroutines
 * to continue their execution. Can also be used to trigger particles such as the Mage's
 * fireball.
 * 
 */

using UnityEngine;

public class SendMessageOnEvent : MonoBehaviour
{
    /// <summary> Sets the animation complete property for the selected player to be false. </summary>
    public void AnimationStart()
    {
        transform.parent.GetComponentInChildren<Humanoid>().SetAnimationComplete(false);
    }

    /// <summary> Sets the animation complete property for the selected player to be true. </summary>
    public void AnimationEnd()
    {
        transform.parent.GetComponentInChildren<Humanoid>().SetAnimationComplete(true);
    }

    /// <summary>
    /// Enum representing the particle to be triggered.
    /// </summary>
    /// Notes: Currently this enum only represents the mage's fireball.
    /// The purpose of this enum is meant as a reminder for future Chase if
    /// the project is approved for 495 Gold, at which point there is the
    /// possibility of more player character's some of which could have their
    /// own projectiles as well.
    public enum ParticleToTrigger
    {
        Fireball
    }

    /// <summary>
    /// Triggers the particle system for the projectile.
    /// </summary>
    /// <param name="particle">The particle to trigger.</param>
    /// Notes: This function is typically used for the projectiles that are
    /// only made up of particle systems and need to executed at certain frames
    /// of the animation, such as the fireball.
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

    /// <summary>
    /// Function used to tell the projectile to begin moving towards target.
    /// </summary>
    /// <param name="particle">The projectile that needs to start moving.</param>
    /// Notes: Pretty much has the same function as the trigger particle function up
    /// above. Used for animation events on certain frames.
    public void ThrowParticle(ParticleToTrigger particle)
    {
        if (particle == ParticleToTrigger.Fireball)
        {
            Mage.Instance.AbilityOneParticle.GetComponent<ProjectileMover>().EnableMove();
        }
    }
}
