using System.Collections.Generic;

public interface IMove
{
    /// <summary> Triggers movement for unit on grid map. </summary>
    void Move(List<Tile> path, bool bypassRangeCheck = false);
}
