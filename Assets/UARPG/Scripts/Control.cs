using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public enum UIFocus
{
    game,
    menu
}

public enum MovementDirection
{
    forward = 1,
    backwards = -1, 
    rigth = 2,
    left = -2
}

public class Control : MonoBehaviour
{
    [SerializeField] UIFocus Focus = UIFocus.game;
    [SerializeField] protected bool horizontalAxisActive = false;
    [SerializeField] protected bool verticalAxisActive = false;

    public UIFocus focus => Focus;
	 
	private void Update ()
    {
        if (Focus == UIFocus.menu)
        {
            if (Input.GetMouseButtonDown(0)) { ScreenMouseClick?.Invoke(Input.mousePosition); }
            else if (Input.GetMouseButtonUp(0)) MouseRelease?.Invoke();
            else if (Input.GetButtonDown(Constants.Input.confirmButton)) ConfirmButtonPressed?.Invoke();
            else if (Input.GetButtonDown(Constants.Input.cancelButton)) CancelButtonPressed?.Invoke();
            else
            {
                float axis;
                if ((axis = Input.GetAxisRaw(Constants.Input.horizontalAxis)) != 0 && !horizontalAxisActive)
                {
                    GuiMovement?.Invoke((MovementDirection)(axis * 2));
                    horizontalAxisActive = true;
                }
                else if ((axis = Input.GetAxisRaw(Constants.Input.verticalAxis)) != 0 && !verticalAxisActive)
                {
                    GuiMovement?.Invoke((MovementDirection)(axis));
                    verticalAxisActive = true;
                }
                if (Input.GetAxisRaw(Constants.Input.horizontalAxis) == 0) horizontalAxisActive = false;
                if (Input.GetAxisRaw(Constants.Input.verticalAxis) == 0) verticalAxisActive = false;
            }                
        }

        UpdateCustom();
    }
    virtual protected void UpdateCustom() { }

    public void ChangeUIFocus(UIFocus focus)
    {
        if (Focus != focus)
        {
            Focus = focus;
            FocusChange?.Invoke(focus);
        }
    }

    public event System.Action ConfirmButtonPressed;
    public event System.Action CancelButtonPressed;
    public event System.Action<MovementDirection> GuiMovement;
    public event System.Action<Vector3> ScreenMouseClick;
    public event System.Action MouseRelease;
    public event System.Action<UIFocus> FocusChange;
}
