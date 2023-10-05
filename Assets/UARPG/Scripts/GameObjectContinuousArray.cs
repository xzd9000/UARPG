using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

[System.Serializable] public class GameObjectContinuousArray
{
    [SerializeField] GameObject[] objects;
    [SerializeField] int size;
    [SerializeField][EditorReadOnly] int Last;

    public int last => Last;

    public GameObjectContinuousArray(int size)
    {
        Last = -1;
        this.size = size;
        objects = new GameObject[size];
    }

    public int Add(GameObject obj)
    {
        if (obj != null)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null)
                {
                    objects[i] = obj;
                    if (i > Last) Last = i;
                    return i;
                }
            }
        }
        return -1;
    }

    public bool Remove(GameObject obj)
    {
        if (obj != null)
        {
            for (int i = 0; i <= last; i++)
            {
                if (objects[i] != null)
                {
                    if (objects[i].gameObject == obj.gameObject)
                    {
                        objects[i] = null;
                        if (i == Last) Last--;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public GameObject GetLast()
    {
        if (last > -1) return objects[last];
        else return null;
    }

    public GameObject this[int index] => objects[index]; 
}
