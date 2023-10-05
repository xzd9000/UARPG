using UnityEngine;
using System.Collections;

#pragma warning disable 0649

public abstract class AI : MonoBehaviour
{
    public AIController aiController { get; private set; }
    public Character character { get; private set; }

    protected virtual void Awake()
    {
        aiController = GetComponent<AIController>();
        character = GetComponent<Character>();
    }

    public abstract void BaseAction();
}
