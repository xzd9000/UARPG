using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    void Move(Vector3 movement);
    void MoveTo(Vector3 position, Vector3 movement);
    void MoveTo(Vector3 position);
}
