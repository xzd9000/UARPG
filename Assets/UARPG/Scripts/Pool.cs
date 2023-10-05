using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class Pool : Singleton<Pool>
{
    [SerializeField][EditorReadOnly] GameObject[] objects = new GameObject[10000];
    [SerializeField][EditorReadOnly] int Last = -1;
    [SerializeField][Min(1)] int size = 10000;

    protected override void AwakeCustom()
    {
        objects = new GameObject[size];
        Last = -1;
    }

    public int last
    {
        get => Last;
        private set
        {
            if (value >= objects.Length - 1)
            {
                Last = (objects.Length - 1);
                full = true;
            }
            else
            {
                Last = value;
                full = false;
            }
        }

    }
    public bool full { get; private set; } = false;

    public void Add(GameObject obj)
    {
        if (IsReusable(obj))
        {           
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null)
                {
                    objects[i] = obj;
                    if (i > last) last = i;
                    return;
                }
            }
            Debug.LogWarning("Pool is full");
        }
    }
    public GameObject Take(int id)
    {
        for (int i = 0; i <= last; i++)
        {
            if (objects[i] != null)
            {
                if (objects[i].GetComponent<Reusable>().id == id)
                {
                    GameObject obj = objects[i];
                    objects[i] = null;
                    if (i == last) last--;
                    return obj;
                }
            }
        }

        return null;
    }

    public static bool IsReusable(GameObject obj)
    {
        try { return obj.GetComponent<Reusable>() != null; }
        finally { if (obj.GetComponent<Reusable>() == null) Debug.LogError(obj + " doesn't have component that inherits from IReusable"); }
    }

    public Pool(int size) => AwakeCustom();

}
