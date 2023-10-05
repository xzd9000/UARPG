using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GUIInteractiveObject))]
public class GUIEventTrigger : EventTrigger
{
    private GUIInteractiveObject interactiveObject;

    private void Awake() => interactiveObject = GetComponent<GUIInteractiveObject>();

    private void Interact(GUIInteractiveObject obj) => ActivateTrigger(obj);

    private void OnEnable() => interactiveObject.Interaction += Interact;
    private void OnDisable() => interactiveObject.Interaction -= Interact;
}