using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable 0649

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshControlledCharacter : Character
{
    [SerializeField] bool modifyAgentRotationSpeed;

    protected NavMeshAgent agent;

    private NavMeshPath path;
    private NavMeshPath currentPath;

    protected override void AwakeCustom()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        OnStatChange();
    }

    protected override void UpdateCustom() => Stop();

    private void OnEnable()
    {
        AttackStart += Stop;
        StatChange += OnStatChange;
    }
    private void OnDisable()
    {
        AttackStart -= Stop;
        StatChange -= OnStatChange;
    }

    private void OnStatChange(object sender = null, System.EventArgs args = null)
    {
        agent.speed = stats.values[moveSpeedStatIndex].value;
        if (modifyAgentRotationSpeed) agent.angularSpeed = stats.values[rotationSpeedStatIndex].value;
    }

    protected override void SetMoveAnimationSpeed()
    {
        if (agent.velocity.sqrMagnitude == 0) anim.SetParameter(Constants.Animator.floatSpeed, 0f);
        else
        {
            if (!scaleMovementAnimationSpeed) anim.SetParameter(Constants.Animator.floatSpeed, 1f);
            else anim.SetParameter(Constants.Animator.floatSpeed, Mathf.Max(agent.velocity.x, agent.velocity.y, agent.velocity.z) * movementAnimationScale);
        }
    }

    protected override void MoveCustom(Vector3 movement)
    {
        if (agent.enabled) agent.Move(movement);
    }
    protected override void MoveToCustom(Vector3 position, Vector3 movement)
    {
        agent.isStopped = false;
        if (agent.destination != position) agent.SetDestination(position);
    }

    public override bool CanReach(Vector3 position) => agent.CalculatePath(position, path);
    
    public override float PathLengthTo(Vector3 position)
    {
        currentPath = agent.path;
        if (agent.CalculatePath(position, path))
        {
            agent.path = path;
            try { return agent.remainingDistance; }
            finally { agent.path = currentPath; }
        }
        return float.PositiveInfinity;
    }

    public void Stop() { if (agent.enabled) agent.isStopped = true; }

    protected override void DeathCustom() => agent.enabled = false; 
    protected override void ResurrectionCustom() => agent.enabled = true;
}
