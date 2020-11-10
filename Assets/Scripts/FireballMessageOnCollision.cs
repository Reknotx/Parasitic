using UnityEngine;

public class FireballMessageOnCollision : MonoBehaviour
{

    private void OnParticleCollision(GameObject other)
    {
        /*transform.parent.*/GetComponent<ParticleSystem>().Stop();
        print("hello");
        transform.root.GetComponentInChildren<Mage>().FireBlastParticle.Play();
        gameObject.SetActive(false);
    }
}
