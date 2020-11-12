using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public Transform target;

    public Transform parentTransform;

    IEnumerator Move()
    {
        Vector3 p0 = transform.position;

        Vector3 p1 = target.transform.position;
        Vector3 p01;
        float startTime = Time.time;

        bool moving = true;

        while (moving)
        {
            float u = Time.time - startTime / 1;
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

    private void OnParticleCollision(GameObject other)
    {
        GetComponent<ParticleSystem>().Stop();
        transform.parent = parentTransform;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        print("Reset parent transform to " + transform.parent);
        Mage.Instance.FireBlastParticle.Play();
        gameObject.SetActive(false);
    }

    public void EnableMove()
    {
        parentTransform = transform.parent;
        print("Parent transform is " + parentTransform.name);
        transform.parent = transform.root;

        StartCoroutine(Move());
    }

}
