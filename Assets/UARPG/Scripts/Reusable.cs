using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class Reusable : MonoBehaviour
{
    [SerializeField] [EditorReadOnly] protected int ID;
    [SerializeField] [EditorReadOnly] protected float LastUseTime;
    [SerializeField] bool initializeID = true;

    public int id => ID;
    public float lastUseTime => LastUseTime;

    protected Renderer[] renderers;
    protected Light[] lights;
    protected Collider collider_;

    [ContextMenu("SetID")]
    private void SetID() => ID = Global.GenerateID();

    private void Awake()
    {
        if (initializeID) ID = (int)(Time.realtimeSinceStartup * 1000000);
        renderers = GetComponentsInChildren<Renderer>();
        lights = GetComponentsInChildren<Light>();
    }

    public GameObject Spawn(Transform parent) => Spawn(parent, Vector3.negativeInfinity);
    public GameObject Spawn(Transform parent, Vector3 startingPosition)
    {
        GameObject obj;
        obj = Pool.instance.Take(ID);
        if (obj == null)
        {
            if (float.IsNegativeInfinity(startingPosition.x)) obj = Instantiate(gameObject, parent);
            else obj = Instantiate(gameObject, startingPosition, Quaternion.identity, parent);
            obj.GetComponent<Reusable>().ID = ID;
        }
        else obj.transform.SetParent(parent);
        obj.SetActive(true);
        return obj;
    }
    public void Remove()
    {
        Removed?.Invoke(gameObject);
        if (!Pool.instance.full)
        {
            Pool.instance.Add(gameObject);
            gameObject.SetActive(false);
            StopAllCoroutines();
        }
        else Destroy(gameObject);
    }

    public event System.Action<GameObject> Removed;
}
