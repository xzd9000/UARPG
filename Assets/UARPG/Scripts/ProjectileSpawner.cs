using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class ProjectileSpawner : AProjectile
{
    [Header("Spawner properties")]
    [SerializeField] ProjectileSpawn[] spawns;
    [SerializeField] float startDelay = 0;
    [SerializeField] int spawnCycles = 1;
    [SerializeField] float spawningTime = 0;
    [SerializeField] bool destroyOnFinish;

    protected bool spawnerActive;
    protected bool spawnActive;
    protected IEnumerator spawnTimer;
    protected IEnumerator spawnerTimer;

    private ProjectileSpawn spawn;
    private AProjectile proj;

    public override HittingObject Activate(Character owner, Vector3 statrtingPoint, Vector3 targetPoint)
    {
        HittingObject obj = base.Activate(owner, statrtingPoint, targetPoint);
        ProjectileSpawner spawner = obj as ProjectileSpawner;
        spawner.StartCoroutine(spawner.Spawning());
        return obj;
    }
    public override void Deactivate()
    {       
        if (spawnTimer != null) StopCoroutine(spawnTimer);
        if (spawnerTimer != null) StopCoroutine(spawnerTimer);
        spawnerActive = false;
        spawnActive = false;
        InvokeDeactivated();
    }

    private IEnumerator Spawning()
    {
        if (startDelay > 0) yield return new WaitForSeconds(startDelay);

        spawnerTimer = SpawnerTimer(spawningTime);
        StartCoroutine(spawnerTimer);

        for (int i = 0; ((i < spawnCycles) || (spawnCycles < 1)) && spawnerActive; i++)
        {
            for (int ii = 0, spawncount; ii < spawns.Length; ii++)
            {
                spawn = spawns[ii];
                spawncount = 0;

                if (spawn.startDelay > 0f) yield return new WaitForSeconds(spawn.startDelay);

                if (spawn.cycling)
                {
                    spawnTimer = SpawnTimer(spawn.duration);
                    StartCoroutine(spawnTimer);
                }
                else spawnActive = true;

                while (spawnActive)
                {
                    for (int iii = 0, index; iii < spawn.projectiles.Length; iii++)
                    {
                        if (!spawn.randomOrder) index = iii;
                        else index = Random.Range(0, spawn.projectiles.Length);

                        proj = spawn.projectiles[index].Activate(owner) as AProjectile;
                        proj.AddTransformation(spawn.transformation * spawncount);
                        spawncount++;

                        if (spawn.spawnDelay > 0f || spawn.spawnDelayRandomRange > 0f) yield return new WaitForSeconds(spawn.spawnDelay + Random.Range(-spawn.spawnDelayRandomRange, spawn.spawnDelayRandomRange));
                    }
                    if ((!spawn.cycling) || (spawn.spawnCount > 0 && spawncount >= spawn.spawnCount) || spawn.projectiles.Length <= 0) spawnActive = false;

                    if (spawn.cycling && spawn.spawnCount <= 0) yield return null;
                }

                if (spawn.endDelay > 0) yield return new WaitForSeconds(spawn.endDelay);
            }
            yield return null;
        }
        if (destroyOnFinish) reusable.Remove();
    }
    private IEnumerator SpawnTimer(float time)
    {
        spawnActive = true;
        if (time > 0f)
        {
            yield return new WaitForSeconds(time);
            spawnActive = false;
        }
        spawnTimer = null;
    }
    private IEnumerator SpawnerTimer(float time)
    {
        spawnerActive = true;
        if (time > 0)
        {
            yield return new WaitForSeconds(time);
            spawnerActive = false;
        }
        spawnerTimer = null;
    }

}
