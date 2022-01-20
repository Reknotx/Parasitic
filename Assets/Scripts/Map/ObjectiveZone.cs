using UnityEngine;

public class ObjectiveZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player)
            {
                print("player enetered objective zone");
                player.InObjectiveZone(this, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player)
            {
                player.InObjectiveZone(this, false);
            }
        }
    }
}
