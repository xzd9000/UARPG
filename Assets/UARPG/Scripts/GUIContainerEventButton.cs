using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class GUIContainerEventButton : GUIEventButton, IGUIDataProvider
{
    [SerializeField][EditorReadOnly] int Index;
                         private bool indexSet; 

    public int index => Index;
    public void InitIndex(int i)
    {
        if (!indexSet)
        {
            Index = i;
            indexSet = true;
        }
    }

    public GUIData guiData { get; set; }
}
