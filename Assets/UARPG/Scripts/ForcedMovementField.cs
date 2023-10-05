using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ForcedMovementField : MonoBehaviour
{
    private enum MovementType
    {
        towardsCenter = 0,
        awayFromCenter = 1,
        relativelyToTarget = 2,
        relativelyToCenter = 3
    }

    [SerializeField] MovementType type;
    [SerializeField][HideUnless("type", typeof(System.Enum), 2, 3)] Vector3 movement;
    [SerializeField] float speed;

    private Dictionary<Collider, IMoveable> moveables = new Dictionary<Collider, IMoveable>();

    private void OnTriggerEnter(Collider other) { if (other.TryGetComponent(out IMoveable moveable)) if (!moveables.ContainsKey(other)) moveables.Add(other, moveable); }
    private void OnTriggerExit(Collider other) { if (moveables.ContainsKey(other)) moveables.Remove(other); }

    private void OnTriggerStay(Collider other)
    {
        if (moveables.ContainsKey(other))
        {
            IMoveable moveable = moveables[other];
            Vector3 movement;
            if (type == MovementType.towardsCenter || type == MovementType.awayFromCenter)
            {
                movement = other.transform.InverseTransformDirection(other.transform.DirectionToTarget(transform.position, true)) * speed * Time.deltaTime * Global.instance.moveSpeedMultiplier;
                if (type == MovementType.awayFromCenter) movement = -movement;
            }
            else
            {
                movement = this.movement * speed * Time.deltaTime * Global.instance.moveSpeedMultiplier;
                if (type == MovementType.relativelyToCenter) movement = other.transform.InverseTransformDirection(transform.TransformDirection(movement));               
            }
            moveable.Move(movement);           
        }
    }
}