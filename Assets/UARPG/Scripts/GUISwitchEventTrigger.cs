using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GUIEventSwitch))]
public class GUISwitchEventTrigger : EventTrigger
{
    [SerializeField] bool triggerOnActivation = true;

    private GUIEventSwitch interactiveObject;

    private void Awake() => interactiveObject = GetComponent<GUIEventSwitch>();

    private void Interact(GUIInteractiveObject obj, bool activation) { if (activation == triggerOnActivation) ActivateTrigger(obj); }

    private void OnEnable() => interactiveObject.SwitchInteraction += Interact;
    private void OnDisable() => interactiveObject.SwitchInteraction -= Interact;
}
