using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAIPatroling : IdleAI
{
    private int waypointIndex = 0;

    [SerializeField] GameObject[] waypoints;

    public override void BaseAction()
    {
        if (Vector3.Distance(transform.position, waypoints[waypointIndex].transform.position) > aiController.destinationReachDistance) character.MoveTo(waypoints[waypointIndex].transform.position);
        else RaiseWaypointIndex();
    }

    private void RaiseWaypointIndex()
    {
        waypointIndex++;
        if (waypointIndex >= waypoints.Length) waypointIndex = 0;
    }
}
