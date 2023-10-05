using System;
using UnityEngine;
using static System.Convert;

#pragma warning disable 0649

[Serializable] public class SaveInfo
{
    [SerializeField] string Name = "default save";
    [SerializeField] int Playtime = 0;
    [SerializeField] int Level = 0;
    [SerializeField] int LastSpawnpointIndex = 0;

    public string lastSceneName;

    public string name => Name;
    public int playTime
    {
        get => Playtime;
        set { if (value > 0) Playtime = value; }
    }
    public int level
    {
        get => Level;
        set { if (value > 0) Level = value; }
    }
    public int lastSpawnPointIndex
    {
        get => LastSpawnpointIndex;
        set { if (value > 0) LastSpawnpointIndex = value; }
    }

    public string MakeSaveData()
    {
        string data = Name + "\n" +
                      Playtime.ToString() + "\n" +
                      Level.ToString() + "\n" +
                      lastSceneName + "\n" +
                      LastSpawnpointIndex.ToString();
        return data;
    }
    public void ReadSaveData(string data)
    {
        string[] lines = data.Split('\n');
        Name = lines[0];
        Playtime = ToInt32(lines[1]);
        Level = ToInt32(lines[2]);
        lastSceneName = lines[3];
        LastSpawnpointIndex = ToInt32(lines[4]);
    }

    public SaveInfo(string name, string lastSceneName, int playtime, int level)
    {
        Name = name;
        this.lastSceneName = lastSceneName;
        Playtime = playtime;
        Level = level;
    }
    public SaveInfo(string saveData) => ReadSaveData(saveData);
}