using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public abstract class GUIInteractiveObject : GUIObject
{
    public abstract void Interact(Vector3 mouseCoordinates);

    protected virtual void OnEnable() => control.ScreenMouseClick += Interact;
    protected virtual void OnDisable() => control.ScreenMouseClick -= Interact;

    public event System.Action<GUIInteractiveObject> Interaction;

    protected void OnInteraction() => Interaction?.Invoke(this);
}