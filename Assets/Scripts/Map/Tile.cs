using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileEffect
{
    None,Dodge,Defense,Attack,Healing
}

public class Tile : MonoBehaviour
{
    /// <summary> Unit is currently occupying space </summary>
    [HideInInspector] public bool occupied = false;

    /// <summary> Tile can be moved over </summary>
    public bool movementTile = true;

    /// <summary> Ranged attacks will pass through tile if false</summary>
    public bool blocksLOS = false;

    bool drawTileGizmo = true;
    private float gizmoHeight = 0.5f;

    public TileEffect tileEffect = TileEffect.None;
    public float effectMagnitude = 0;
    public int effectCooldown = 10;
    public int remainingCooldown = 0;

    //used for pathfinding
    [HideInInspector]
    public Vector2 gridPosition;
    [HideInInspector]
    public int gCost = 0;
    [HideInInspector]
    public int hCost = 0;
    [HideInInspector]
    public Tile parent;
    //reference to unit currently on tile
    [HideInInspector]
    public Humanoid occupant;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public float TileBoost(TileEffect desiredBoost)
    {
        if (desiredBoost == tileEffect)
        {
            return effectMagnitude;
        }
        else
        {
            return 0f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (!movementTile)
        {
            Gizmos.color = Color.red;
            if (!blocksLOS)
            {
                Gizmos.color = Color.yellow;
            }
            
        }
        else
        {
            if (tileEffect == TileEffect.Dodge)
            {
                Gizmos.color = new Color(0, 0.5f, 0, 1);
            }
            else if (tileEffect == TileEffect.Defense)
            {
                Gizmos.color = Color.blue;
            }
            else if (tileEffect == TileEffect.Attack)
            {
                Gizmos.color = Color.white;
            }
            else if (tileEffect == TileEffect.Healing && remainingCooldown == 0)
            {
                Gizmos.color = Color.magenta;
            }
            if (blocksLOS)
            {
                Gizmos.color = Color.black;
            }
        }
        if (drawTileGizmo)
            Gizmos.DrawSphere(transform.position + Vector3.up*gizmoHeight, 0.2f);
    }

    public void StartCooldown()
    {
        remainingCooldown = effectCooldown;
    }

    public bool NewRound()
    {
        remainingCooldown--;

        if(remainingCooldown == 0)
        {
            return true;
        }
        return false;
    }

}
