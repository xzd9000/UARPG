using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    [SerializeField] Event[] events;

    public void ActivateTrigger(params object[] data) { for (int i = 0; i < events.Length; i++) events[i].ActivateTrigger(this, data); }
}