using System;
using UnityEngine;
using System.Collections;

#pragma warning disable 0649

[RequireComponent(typeof(Collider))]
public abstract class InteractiveObject : MonoBehaviour
{
    [SerializeField] bool questNotifications = false;

    new protected Collider collider { get; private set; }

    private void Awake()
    {
        collider = GetComponent<Collider>();
        AwakeCustom();
    }
    protected virtual void AwakeCustom() { }

    public void Interact() => Interact(null, EventArgs.Empty);
    public virtual void Interact(object sender, EventArgs args)
    {
        Interaction?.Invoke(sender, EventArgs.Empty);
        if (questNotifications) QuestManager.instance.NotyifyInteraction(this);
    }
    public event EventHandler Interaction;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Character character)) character.AddInteractiveObject(this);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Character character)) character.RemoveInteractiveObject(this);
    }
}
