using UnityEngine;

public class GUIFollowCursor : GUIObject
{
    private void Update()
    {
        rect.position = Input.mousePosition;
    }
}