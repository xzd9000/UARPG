using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class ProjectileSpawn 
{
    public bool randomOrder;  
    public AProjectile[] projectiles;
    public ProjectileTransformation transformation;
    public float duration;
    public int spawnCount;
    public float startDelay;
    public float spawnDelay;
    public float spawnDelayRandomRange;
    public float endDelay;
    public bool cycling;
}
