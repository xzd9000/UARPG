using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIEventSwitch : GUIEventButton
{
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color activeColor = Color.cyan;

    public bool activated { get; private set; }

    public override void Interact(Vector3 mouseCoordinates)
    {
        if (IsWithinWindow(mouseCoordinates))
        {
            OnInteraction();
            activated = !activated;
            SetColorAll(activated ? activeColor : normalColor);
            SwitchInteraction?.Invoke(this, activated);
        }
    }

    public event System.Action<GUIInteractiveObject, bool> SwitchInteraction;
}
