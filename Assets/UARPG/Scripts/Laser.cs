using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class Laser : AProjectile
{
    private LineRenderer line;
    private RaycastHit hit;
    private bool contactCheck;

    [Header("Laser Properties")]
    [SerializeField][EditorReadOnly] float timeLeft;
    [SerializeField] float width = 0f;
    [SerializeField] float duration = 10f;
    [SerializeField] float maxLength = 500f;
    [SerializeField] LayerMask raycastLayers = Physics.DefaultRaycastLayers;
    [SerializeField] bool destroyOnFinish;

    protected override void AwakeCustom() => line = GetComponent<LineRenderer>();
    

    public override HittingObject Activate(Character owner, Vector3 startingPoint, Vector3 targetPoint)
    {
        Laser obj = base.Activate(owner, startingPoint, targetPoint) as Laser;
        obj.contactCheck = true;
        laserUpdate = obj.LaserUpdate();
        obj.StartCoroutine(laserUpdate);
        return obj;
    }
    public override void Deactivate()
    {
        if (laserUpdate != null) StopCoroutine(laserUpdate);
        laserUpdate = null;
        InvokeDeactivated();
    }

    public override void Disable() => contactCheck = false; 
    public override void Deflect() { }

    private IEnumerator laserUpdate;
    private IEnumerator LaserUpdate()
    {
        timeLeft = duration > 0 ? duration : float.PositiveInfinity;
        float time = attackDelay > 0 ? attackDelay : Global.instance.laserUpdateRate;
        int positionCount = line.positionCount;
        float lineLength;
        Vector3 lineOldPosition;
        Vector3 selectedPosition;
        float lineDifference;

        while (timeLeft > 0)
        {
            if (width > 0) Physics.SphereCast(transform.position, width, transform.forward, out hit, maxLength, raycastLayers);
            else Physics.Raycast(transform.position, transform.forward, out hit, maxLength, raycastLayers);
            if (hit.collider is Collider)
            {
                lineLength = hit.distance;
                if (contactCheck) Hit(hit.collider.gameObject, hit.point);
            }
            else lineLength = maxLength;

            lineOldPosition = line.GetPosition(positionCount - 1);
            line.SetPosition(positionCount - 1, new Vector3(lineOldPosition.x, lineOldPosition.y, lineLength));
            lineDifference = line.GetPosition(positionCount - 1).z / lineOldPosition.z;
            for (int i = 1; i < positionCount - 1; i++)
            {
                selectedPosition = line.GetPosition(i);
                line.SetPosition(i, new Vector3(selectedPosition.x, selectedPosition.y, selectedPosition.z * lineDifference));
            }

            yield return new WaitForEndOfFrame();
            timeLeft -= Time.deltaTime;
        }

        laserUpdate = null;
        if (destroyOnFinish) reusable.Remove();
    }
}
