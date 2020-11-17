using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAtTarget : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        print("Collided with " + other.name);

        if (other.GetComponent<Humanoid>() != null && other.GetComponent<Player>() == CharacterSelector.Instance.SelectedTargetUnit)
        {
            print("We hit it");
            Archer.Instance.potionHitTarget = true;
            //gameObject.SetActive(false);

            gameObject.GetComponent<MeshRenderer>().enabled = false;
            GetComponentInChildren<ParticleSystem>().Stop();

            StartCoroutine(Reset());

        }
    }

    IEnumerator Reset()
    {
        ParticleSystem potionTrail = GetComponentInChildren<ParticleSystem>();

        yield return new WaitUntil(() => potionTrail.isStopped);

        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.SetActive(false);
    }

    IEnumerator Move()
    {
        Vector3 p0 = transform.position;

        Vector3 p1 = CharacterSelector.Instance.SelectedTargetUnit.transform.position;
        Vector3 p01;
        float startTime = Time.time;

        bool moving = true;

        while (moving)
        {
            float u = (Time.time - startTime / 1) * 2;
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

    public void EnableMove()
    {
        transform.parent = transform.root;

        StartCoroutine(Move());
    }
}
