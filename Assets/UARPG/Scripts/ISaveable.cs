using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveDataProvider
{
    string MakeSaveData();
    void ReadSaveData(string saveData);
}

public interface ISaveable : ISaveDataProvider
{
    bool save { get; }
}
