using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public class GUIEventButton : GUIInteractiveObject
{
    public override void Interact(Vector3 mouseCoordinates)
    {
        if (IsWithinWindow(mouseCoordinates)) OnInteraction();        
    }
}
