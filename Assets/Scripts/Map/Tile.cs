using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (drawTileGizmo)
            Gizmos.DrawSphere(transform.position + Vector3.up*gizmoHeight, 0.2f);
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}
