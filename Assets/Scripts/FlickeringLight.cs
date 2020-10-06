using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    public Light light;

    //min/max intensity
    public float minIntensity = 2f;
    public float maxIntensity = 4f;

    float random;

    void Start()
    {
        random = Random.Range(0.0f, 600.0f);
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        float noise = Mathf.PerlinNoise(random, Time.time);
        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
