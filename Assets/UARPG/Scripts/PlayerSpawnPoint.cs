using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] bool default_;
    public bool defaultSpawnPoint => default_;
}
