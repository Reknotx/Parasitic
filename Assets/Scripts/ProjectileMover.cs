/*
 * Author: Chase O'Connor
 * Date: 11/10/2020
 * 
 * Brief: Moves a player projectile towards a target. Once the target
 * is reached, a signal can be sent back to the casting player to allow
 * the rest of the damage logic to be executed and applied.
 */

using System.Collections;
using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    /// <summary>
    /// Enum representing the owner of the projectile. Probably useless honestly.
    /// </summary>
    public enum Owner
    { 
        Warrior,
        Mage,
        Archer
    }

    /// <summary>
    /// Enum representing the type of the projectile that is in action. Dictates
    /// the execution of logic.
    /// </summary>
    public enum ProjectileType
    { 
        ArcherPotion,
        ArcherArrow,
        MageFireball
    }

    [Header("Who casts this projectile.")]
    /// <summary> Who casts this projectile. </summary>
    public Owner owner;

    [Space]
    [Header("The type of projectile being cast.")]
    /// <summary> The type of projectile being cast. </summary>
    public ProjectileType projectileType;

    [Space]
    [Header("The transform of the target. Can be set either in code or in the inspector.")]
    /// <summary> The transform of the target. Can be set either in code or in the inspector. </summary>
    public Transform target;

    [Space]
    [Header("The parent transform of the projectile before casting.")]
    /// <summary> The parent transform of the projectile before casting. </summary>
    public Transform parentTransform;

    [Range(1, 5)]
    [Header("The speed of the projectile.")]
    public int SpeedModifer = 1;

    /// <summary> Coroutine moving the projectile towards the target. </summary>
    IEnumerator Move()
    {
        Vector3 p0 = transform.position;

        Vector3 p1 = target.transform.position;
        Vector3 p01;
        float startTime = Time.time;

        bool moving = true;

        if (target != null)
        {
            while (moving)
            {
                float u = (Time.time - startTime / 1) * SpeedModifer;
                if (u >= 1)
                {
                    u = 1;
                    moving = false;
                }
                p01 = (1 - u) * p0 + u * p1;
                gameObject.transform.position = p01;
                yield return new WaitForFixedUpdate();

            }
        }
        else
        {
            Debug.LogError("CHASE YOU FUCKING IDIOT ASSIGN THE TARGET!!!!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("Collided with " + other.name);

        if (projectileType == ProjectileType.ArcherPotion)
        {
            if (other.GetComponent<Player>() != null && other.GetComponent<Player>() == CharacterSelector.Instance.SelectedTargetUnit)
            {
                print("We hit it");
                Archer.Instance.potionHitTarget = true;
                //gameObject.SetActive(false);

                gameObject.GetComponent<MeshRenderer>().enabled = false;
                GetComponentInChildren<ParticleSystem>().Stop();

                StartCoroutine(ResetPotion());
            }
        }
        else if (projectileType == ProjectileType.ArcherArrow)
        {
            if (other.GetComponent<Enemy>() != null && other.GetComponent<Enemy>() == CharacterSelector.Instance.SelectedTargetUnit)
            {
                print("Target is hit");
                Archer.Instance.arrowHitTarget = true;
                gameObject.GetComponent<MeshRenderer>().enabled = false;
                StartCoroutine(ResetArrow());
            }
        }
    }

    /// <summary>
    /// Used for the projectiles that are mainly particle systems such as the fireball.
    /// </summary>
    /// <param name="other">The object that was collided with.</param>
    private void OnParticleCollision(GameObject other)
    {
        GetComponent<ParticleSystem>().Stop();

        if (owner == Owner.Mage)
        {
            ResetFireball();
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the target of the projectile, can be either player or enemy.
    /// </summary>
    /// <param name="target">The target of the projectile. Transform is gained from game object.</param>
    public void SetTarget(Humanoid target)
    {
        this.target = target.transform;
    }

    /// <summary>
    /// Triggers the movement coroutine to move the projectile.
    /// </summary>
    public void EnableMove()
    {
        if (parentTransform == null)
        {
            parentTransform = transform.parent;
        }
        print("Parent transform is " + parentTransform.name);
        transform.parent = transform.root;

        StartCoroutine(Move());
    }

    /// <summary>
    /// Called to reset the fireball to it's original position. The fireblast
    /// particle system is also played to complete the animation effect.
    /// </summary>
    void ResetFireball()
    {
        transform.parent = parentTransform;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        Mage.Instance.FireBlastParticle.Play();
    }

    /// <summary>
    /// Coroutine for resetting the position of the potion. Waits until the 
    /// potion trail particle system is ended so that a smoother transition can occur.
    /// </summary>
    IEnumerator ResetPotion()
    {
        ParticleSystem potionTrail = GetComponentInChildren<ParticleSystem>();

        yield return new WaitUntil(() => potionTrail.isStopped);

        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Coroutine for resetting the position of the arrow. Waits until the
    /// arrow trail particle system is ended so that a smoother transition can occur.
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetArrow()
    {
        ParticleSystem[] arrowTrails = GetComponentsInChildren<ParticleSystem>();
        ParticleSystem activeTrail = null;

        foreach (ParticleSystem particleSystem in arrowTrails)
        {
            if (particleSystem.isPlaying)
            {
                particleSystem.Stop();
                activeTrail = particleSystem;
                break;
            }
        }

        yield return new WaitUntil(() => activeTrail.isStopped);

        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.SetActive(false);
    }
}
