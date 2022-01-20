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
    /// Enum representing the owner of the projectile. Used in ensuring the parent
    /// transform of the projectile is properly set at all times.
    /// </summary>
    public enum Owner
    {
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

    /// <summary> Who casts this projectile. </summary>
    [Header("Who casts this projectile.")]
    public Owner owner;

    /// <summary> The type of projectile being cast. </summary>
    [Space, Header("The type of projectile being cast.")]
    public ProjectileType projectileType;

    /// <summary> The transform of the target. Can be set either in code or in the inspector. </summary>
    [Space, Header("The transform of the target. Can be set either in code or in the inspector.")]
    public Transform target;

    /// <summary> The parent transform of the projectile before casting. </summary>
    [Space, Header("The parent transform of the projectile before casting.")]
    public Transform parentTransform;

    [Range(1, 5), Header("The speed of the projectile.")]
    public int SpeedModifer = 1;

    private Vector3 _potionPos = Vector3.zero;


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
        //print("Collided with " + other.name);

        if (projectileType == ProjectileType.ArcherPotion)
        {
            if (other.GetComponent<Player>() != null && other.GetComponent<Player>() == CharacterSelector.Instance.SelectedTargetUnit)
            {
                //print("We hit it");
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
                //print("Target is hit");
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
    public void SetTarget(Humanoid target) => this.target = target.transform;

    /// <summary>
    /// Triggers the movement coroutine to move the projectile.
    /// </summary>
    public void EnableMove()
    {
        if (parentTransform == null)
        {
            parentTransform = transform.parent;
        }
        //print("Parent transform is " + parentTransform.name);
        switch (owner)
        {
            case Owner.Mage:
                transform.parent = Mage.Instance.parentTransform;
                break;

            case Owner.Archer:
                if (projectileType == ProjectileType.ArcherPotion)
                {
                    _potionPos = transform.localPosition;
                }
                transform.parent = Archer.Instance.parentTransform;
                break;

            default:
                break;
        }

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

        transform.parent = parentTransform;

        transform.localPosition = _potionPos;
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
