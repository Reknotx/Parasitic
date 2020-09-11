using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queue
{
    private List<Humanoid> _queue;


    public Queue()
    {
        _queue = new List<Humanoid>();
    }

    public void Append(Humanoid character)
    {
        _queue.Add(character);
    }

    public void AdvanceOrder()
    {
        _queue.Add(_queue[0]);
        _queue.RemoveAt(0);
    }

    public Humanoid NextCharacter()
    {
        return _queue[0];
    }
}
