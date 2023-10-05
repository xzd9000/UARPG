using UnityEngine;
using System.Collections;

#pragma warning disable 0649

public class GUITextField : GUIInteractiveObject
{
    private const int alphabetStartCapitals = 0x0041;
    private const int alphabetStartSmalls = 0x0061;

    public string text
    {
        get => texts[0].text;
        private set => texts[0].text = value;
    }

    [SerializeField] bool listenForTextInput;
    [Header("")]
    [SerializeField] int maxLength = 10;
    [SerializeField] bool acceptNumbers = true;
    [SerializeField] Color activeColor = new Color(182, 255, 43);
    [SerializeField] Color inactiveColor = Color.white;

    protected override void AwakeCustom()
    {
        if (texts.Length < 1) throw new MissingComponentException("GUITextField requires at least one text component on object or in its children");
        else text = "";
        
    }

    void Update()
    {
        if (listenForTextInput)
        {
            if (text.Length <= maxLength)
            {
                for (int i = 48; i < 265;)
                {
                    if (i <= 57)
                    {
                        if (acceptNumbers) { if (Input.GetKeyDown((KeyCode)i)) text += (i - 48).ToString(); }
                        else
                        {
                            i = 97;
                            continue;
                        }
                    }
                    else if (i >= 97 && i <= 122)
                    {
                        if (Input.GetKeyDown((KeyCode)i))
                        {
                            int alphabetStart;
                            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) alphabetStart = alphabetStartCapitals;
                            else alphabetStart = alphabetStartSmalls;
                            text += (char)(i - 97 + alphabetStart);
                        }
                    }
                    else if (i >= 256)
                    {
                        if (acceptNumbers) { if (Input.GetKeyDown((KeyCode)i)) text += (i - 256).ToString(); }
                        else break;
                    }
                    i++;
                }
            }
            if (Input.GetKey(KeyCode.Backspace)) if (text.Length > 0) text = text.Remove(text.Length - 1);
        }
    }

    public override void Interact(Vector3 mouseCoordinates)
    {
        if (IsWithinWindow(mouseCoordinates))
        {
            SetColorAll(activeColor);
            listenForTextInput = true;
        }
        else
        {
            SetColorAll(inactiveColor);
            listenForTextInput = false;
        }
    }

}
