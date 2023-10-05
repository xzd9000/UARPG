using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeEventTrigger : EventTrigger
{
    private void Awake() => ActivateTrigger(gameObject);
}
