using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSBurner : MonoBehaviour
{
    [SerializeField] int cycles = 1;

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            for (int i = 0; i < cycles; i++) Compare();
        }
    }

    private bool Compare() => GetComponent<Transform>() == null;
}
