using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

[System.Flags]
public enum Axis
{
    X = 0b10,
    Y = 0b01,
    XY = 0b11
}

public class MouseLook : MonoBehaviour
{    

    public Axis axis;

    [SerializeField] float sens = 6;
    [SerializeField] float vertMin = -90;
    [SerializeField] float vertMax = 90;
    [SerializeField] bool controlDistance = true;
    [SerializeField][HideUnless("controlDistance", true)] float maxDistance = -30;
    [SerializeField][HideUnless("controlDistance", true)] float defaultCameraDistance = -5;
    [SerializeField][HideUnless("controlDistance", true)] float minDistance = 0;
    [SerializeField][HideUnless("controlDistance", true)] float distanceChangeSpeed = 1;

    private bool mouseLocked;

    private GameObject camera_;
    public float angleY;
    public float angleX;


	// Use this for initialization
	void Start () 
    {
        camera_ = GetComponentInChildren<Camera>().gameObject;
        SetCursorLock(true);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (mouseLocked == true)
        {                     
            if (axis.HasFlag(Axis.Y))
            {
                float mouseY = (Input.GetAxis(Constants.Input.mouseYAxis) * sens);
                angleY -= mouseY;
                angleY = Mathf.Clamp(angleY, vertMin, vertMax);
            }
            else angleY = transform.eulerAngles.x;

            if (axis.HasFlag(Axis.X))
            {
                float mouseX = (Input.GetAxis(Constants.Input.mouseXAxis) * sens);
                angleX = transform.eulerAngles.y + mouseX;
            }
            else angleX = transform.eulerAngles.y;

            SetAngles();

            if (controlDistance)
            {
                float distance = camera_.transform.localPosition.z;
                if (Input.GetAxis(Constants.Input.mouseWheelAxis) > 0) distance += distanceChangeSpeed;
                else if (Input.GetAxis(Constants.Input.mouseWheelAxis) < 0) distance -= distanceChangeSpeed;
                if (distance > minDistance) distance = minDistance;
                if (distance < maxDistance) distance = maxDistance;
                camera_.transform.localPosition = new Vector3(0, 0, distance);
            }
        }
    }

    public void ResetAxis(Axis axis)
    {
        if (axis.HasFlag(Axis.X)) angleY = 0f;
        if (axis.HasFlag(Axis.Y)) angleX = 0f;
    }
    public void SetAngles() => transform.eulerAngles = new Vector3(angleY, angleX, transform.eulerAngles.z);

    public void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
        mouseLocked = locked;
    }

    public void ResetCameraDistance() => camera_.transform.localPosition = new Vector3(0, 0, defaultCameraDistance);
}
