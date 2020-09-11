using UnityEngine;

public interface IMove
{
    /// <summary>
    /// Triggers movement for unit on grid map.
    /// </summary>
    /// <param name="start">Starting position of unit.</param>
    /// <param name="target">Target position of unit.</param>
    void Move(Transform start, Transform target);
}
