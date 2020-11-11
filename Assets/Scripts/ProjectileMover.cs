using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    public enum Owner
    { 
        Warrior,
        Mage,
        Archer
    }

    public enum ProjectileType
    { 
        Potion,
        Arrow,
        Fireball
    }

    public Owner owner;
    public ProjectileType projectileType;

    public Transform target;

    public Transform parentTransform;

    [Range(1, 5)]
    [Header("The speed of the projectile.")]
    public int SpeedModifer = 1;

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

        if (projectileType == ProjectileType.Potion)
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
        else if (projectileType == ProjectileType.Arrow)
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

    public void SetTarget(Humanoid target)
    {
        this.target = target.transform;
    }

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

    void ResetFireball()
    {
        transform.parent = parentTransform;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        Mage.Instance.FireBlastParticle.Play();
    }

    IEnumerator ResetPotion()
    {
        ParticleSystem potionTrail = GetComponentInChildren<ParticleSystem>();

        yield return new WaitUntil(() => potionTrail.isStopped);

        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.SetActive(false);
    }

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
