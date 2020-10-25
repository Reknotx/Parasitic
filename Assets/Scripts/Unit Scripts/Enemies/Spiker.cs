using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiker : Enemy
{
    #region Combat Functions
    public override void Attack()
    {
        Debug.Log("Spiker Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Spiker Defend");
        base.Defend();
    }
    #endregion

    public override void Move(List<Tile> path)
    {
        if (CheckIfInRangeOfTarget() == false)
        {
            base.Move(path);
        }
        else
        {
            // Perform the unique spiker functions.
        }
    }

    enum CloserIn { X, Y }

    public void FindOptimalMaxRangePos()
    {
        if (_currTarget == null)
        {
            FindNearestPlayer();
        }

        ///Perhaps find which direction is closer to the player?
        ///Like if we are closer in x and farther away in the Y then
        ///move away from the player in the X direction. 
        ///Vice versa for a situation when we are closer in the Y but farther 
        ///in the X
        ///
        Coord playerCoord = new Coord(0, 0);
        Coord spikerCoord = new Coord(0, 0);

        playerCoord.X = (int)_currTarget.currentTile.gridPosition.x;
        playerCoord.Y = (int)_currTarget.currentTile.gridPosition.y;

        spikerCoord.X = (int)currentTile.gridPosition.x;
        spikerCoord.Y = (int)currentTile.gridPosition.y;

        int xDelta = Mathf.Abs(playerCoord.X - spikerCoord.X);
        int yDelta = Mathf.Abs(playerCoord.Y - spikerCoord.Y);



    }

    void Notes()
    {
        /*
         * 1. If the spiker is not in range then he needs to move towards
         * a player until he is in range. This is already dealt with with 
         * the normal behaviour of the parent scripts.
         * 
         * 2. If Spiker is within range, but not at MAXIMUM range from the
         * closest player, then the spiker needs to move until they are at 
         * max range from the player
         * 
         * 3. We can just move in any direction that keeps us within range
         * and moving away from the player, depending on current X and Y
         * position on the grid.
         * 
         * 
         * 
         */

    }
    //public override void Dodge()
    //{
    //    Debug.Log("Spiker Dodge");
    //    base.Dodge();
    //}

    private class Coord
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

}
