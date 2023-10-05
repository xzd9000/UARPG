using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#pragma warning disable 0649

[RequireComponent(typeof(Collider))]
public class InfoBox : MonoBehaviour
{

    [SerializeField] GameObjectContinuousArray infos = new GameObjectContinuousArray(100);
    [SerializeField] float reenableTime;

    public GameObject this[int index] => infos[index];
    public int ObjectsLength => infos.last + 1;

    private Collider collider_;
    private IEnumerator cooldown;

    private void Awake()
    {
        collider_ = GetComponent<Collider>();
        cooldown = Cooldown();
    }

    public event System.EventHandler InfoEnter;
    public event System.EventHandler InfoExit;
    public event System.Action<bool> Enabled;

    public void Enable()
    {
        collider_.enabled = true;
        Enabled?.Invoke(true);
        StopCoroutine(cooldown);
    }
    public void Disable()
    {
        collider_.enabled = false;
        Enabled?.Invoke(false);
        if (reenableTime > 0) StartCoroutine(cooldown);      
    }
    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(reenableTime);
        Enable();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        int index = infos.Add(other.gameObject);
        if (index != -1) InfoEnter?.Invoke(this, new ObjectEventArgs<GameObject, int>(other.gameObject, index));
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (infos.Remove(other.gameObject)) InfoExit?.Invoke(this, new ObjectEventArgs<GameObject>(other.gameObject));
    }
}
