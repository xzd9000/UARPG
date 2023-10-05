using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InfoBox))]
public class InfoBoxEventTrigger : EventTrigger
{
    [SerializeField] bool infoEnter = true;
    [SerializeField] bool infoExit;

    private InfoBox infoBox;

    private void Awake() => infoBox = GetComponent<InfoBox>();

    private void InfoEnter(object o, System.EventArgs args) => ActivateTrigger(infoBox, (args as ObjectEventArgs<GameObject, int>).obj1);
    private void InfoExit(object o, System.EventArgs args) => ActivateTrigger(infoBox, (args as ObjectEventArgs<GameObject>).obj);

    private void OnEnable()
    {
        if (infoEnter) infoBox.InfoEnter += InfoEnter;
        if (infoExit) infoBox.InfoExit += InfoExit;
    }
    private void OnDisable()
    {
        if (infoEnter) infoBox.InfoEnter -= InfoEnter;
        if (infoExit) infoBox.InfoExit -= InfoExit;
    }
}