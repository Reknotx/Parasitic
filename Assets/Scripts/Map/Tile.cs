﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    bool occupied = false;
    public bool movementTile = true;
    bool drawTileGizmo = true;
    private float gizmoHeight = 0.5f;

    //used for pathfinding
    [HideInInspector]
    public Vector2 gridPosition;
    [HideInInspector]
    public int gCost;
    [HideInInspector]
    public int hCost;
    [HideInInspector]
    public Tile parent;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (!movementTile)
        {
            Gizmos.color = Color.red;
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
