using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public Transform firePlane;


    IEnumerator Move()
    {
        Vector3 p0 = transform.position;

        Vector3 p1 = firePlane.transform.position;
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

    private void OnEnable()
    {
        StartCoroutine(Move());
    }

}
