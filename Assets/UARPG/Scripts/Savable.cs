using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Convert;

#pragma warning disable 0649

public class Savable : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] bool saveDestroyedState;
    [SerializeField] bool saveTransform;

    private bool destroyed = false;

    public int id => ID;

    public const string endObject = "endObj";
    public const string nullStr = "null";
    public const string endComponent = "endcomponent";

    public string GetSaveData()
    {
        string ret = ID.ToString() + "\n"
                   + destroyed.ToString() + "\n";
        if (destroyed == false)
        {
            if (saveTransform) ret += transform.position.x.ToString() + " " + transform.position.y.ToString() + " " + transform.position.z.ToString() + "\n" +
                                      transform.localScale.x.ToString() + " " + transform.localScale.y.ToString() + " " + transform.localScale.z.ToString() + "\n" +
                                      transform.eulerAngles.x.ToString() + " " + transform.eulerAngles.y.ToString() + " " + transform.eulerAngles.z.ToString() + " ";
            else ret += nullStr + "\n\n\n";
            Component[] components = GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is ISaveable saveable)
                {
                    if (saveable.save)
                    {
                        ret += i.ToString() + "\n" +
                         saveable.MakeSaveData() + "\n" +
                         endComponent + "\n";
                    }
                }
            }
        }
        ret += endObject;
        return ret;
    }
    public void Load(string saveData)
    {
        string[] lines = saveData.Split('\n');
        if (ToBoolean(lines[1]) == true)
        {
            Destroy(this);
            return;
        }
        if (lines[2] != nullStr)
        {
            string[] words;
            Vector3 vector;
            for (int i = 3; i < 6; i++)
            {
                words = lines[i].Split(' ');
                vector.x = ToSingle(lines[0]);
                vector.y = ToSingle(lines[1]);
                vector.z = ToSingle(lines[2]);
                switch (i)
                {
                    case 1: transform.position = vector; break;
                    case 2: transform.localScale = vector; break;
                    case 3: transform.eulerAngles = vector; break;
                }
            }
        }
        Component[] components = GetComponents<Component>();
        string data;
        int index;
        for (int i = 5; lines[i] != endObject;)
        {
            data = "";
            index = ToInt32(lines[i]);
            for (i++; lines[i] != endComponent; i++) data += lines[i];
            (components[i] as ISaveable).ReadSaveData(data);
            i++;
        }
    }

    private void OnDestroy()
    {
        if (saveDestroyedState)
        {
            destroyed = true;
            LoadManager.instance.AddDestroyedObjectSaveData(GetSaveData());
        }
    }
}
