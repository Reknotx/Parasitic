using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public List<Humanoid> players;

    // Start is called before the first frame update
    void Start()
    {
        foreach (IPlayer player in players)
        {
            //player.NormalAttack();
            //player.AbilityOne();
            //player.AbilityTwo();
        }

        foreach (IStatistics playerStats in players)
        {
            Debug.Log(playerStats.Health);
        }

        foreach (IMove moveable in players)
        {
            moveable.Move();
        }
    }
}
