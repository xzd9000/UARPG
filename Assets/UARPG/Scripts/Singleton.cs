using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

[DisallowMultipleComponent]
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] bool dontDestroyOnLoad = true;

    public static T instance;

    void Awake()
    {

        if (instance != null)
        {
            if (dontDestroyOnLoad) Destroy(gameObject);
            else Destroy(this);
        }
        else instance = this as T;

        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        AwakeCustom();
    }

    protected virtual void AwakeCustom() { }

}
