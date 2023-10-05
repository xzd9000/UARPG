//#define nocreate
//#define debugCreateFolders

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.SceneManagement.SceneManager;
using static System.Convert;

#pragma warning disable 0649

public class LoadManager : Singleton<LoadManager>, IGUIDataList
{
    [SerializeField][EditorReadOnly] List<SaveInfo> saveInfos;
    [SerializeField]                 int spawnPointIndex = -1;
    [SerializeField]                 GameObject playerAsset;
    [SerializeField]                 PlayerSpawnPoint[] spawnPoints;
    [SerializeField]                 List<GUIData> GuiData;

    private SaveInfo currentSaveInfo = null;
    private List<Savable> savables = new List<Savable>();
    private List<string> destroyedSaveData = new List<string>();
    private Player player;
    private Savable playerSavable;
    private Scene mainMenuScene;

    public List<GUIData> guiData => GuiData;

    private List<string> saveFolders;
    private string saveFolder;
    private string savePath => Global.instance.savesPath + "\\" + saveFolder;
    private string playerPath => savePath + "\\player";
    private string scenePath => savePath + "\\" + GetActiveScene().name;
    private string saveInfoPath => savePath + "\\save.info";
    private float lastSaveTime;

    protected override void AwakeCustom()
    {
        mainMenuScene = GetActiveScene();
        if (!Directory.Exists(Global.instance.savesPath))
        {
            #if nocreate
            Debug.LogWarning("Saves directory is not created, saves cannot be read");
            return;
            #endif
            Directory.CreateDirectory(Global.instance.savesPath);
            Debug.LogWarning("New directory was created:" + Global.instance.savesPath);
        }
        saveFolders = new List<string>();
        saveFolders.AddRange(Directory.GetDirectories(Global.instance.savesPath));
        saveInfos = new List<SaveInfo>();
        GuiData = new List<GUIData>();
        string data;
        for (int i = 0; i < saveFolders.Count; i++)
        {
            data = File.ReadAllText(saveFolders[i] + "\\save.info");
            saveInfos.Add(new SaveInfo(data));         
        }

        #if debugCreateFolders
        for (int i = 1; i <= 20; i++) AddSaveInfo(new SaveInfo(i.ToString(), "SampleScene", i, UnityEngine.Random.Range(0, 101)));
        #endif

        UpdateGUIData();
    }

    private void Start() => sceneLoaded += OnSceneLoad;

    public void LoadSave(int saveSlotIndex)
    {
        if (TrySelectSaveInfo(saveSlotIndex))
        {
            string sceneName = currentSaveInfo.lastSceneName;
            spawnPointIndex = currentSaveInfo.lastSpawnPointIndex;
            LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode loadMode)
    {
        if (loadMode == LoadSceneMode.Single && scene != mainMenuScene)
        {
            lastSaveTime = Time.realtimeSinceStartup;
            foreach (Savable savable in Resources.FindObjectsOfTypeAll<Savable>()) if (savable.gameObject.tag != Constants.Tags.player) savables.Add(savable);
            spawnPoints = Resources.FindObjectsOfTypeAll<PlayerSpawnPoint>();
            if (spawnPoints.Length > 0)
            {
                if (spawnPointIndex >= 0) Load(spawnPoints[spawnPointIndex].gameObject);
                else
                {
                    int defaultIndex = 0;
                    for (int i = 0; i < spawnPoints.Length; i++)
                    {
                        if (spawnPoints[i].defaultSpawnPoint)
                        {
                            defaultIndex = i;
                            break;
                        }
                    }
                    Load(spawnPoints[defaultIndex].gameObject);
                }
            }
        }
    }

    public void Save()
    {
        if (currentSaveInfo != null)
        {
            string saveData = "";
            foreach (Savable savable in savables) saveData += savable.GetSaveData() + "\n";
            foreach (string data in destroyedSaveData) saveData += data;
            File.WriteAllText(scenePath, saveData);
            if (playerSavable != null) File.WriteAllText(playerPath, playerSavable.GetSaveData());

            currentSaveInfo.playTime += (int)(Time.realtimeSinceStartup - lastSaveTime);
            currentSaveInfo.lastSceneName = GetActiveScene().name;
            currentSaveInfo.level = player.level;

            File.WriteAllText(saveInfoPath, currentSaveInfo.MakeSaveData());
        }
    }


    public void Load(GameObject playerSpawn)
    {
        GameObject player_ = Instantiate(playerAsset, playerSpawn.transform.position, playerSpawn.transform.rotation);
        player = player_.GetComponent<Player>();
        playerSavable = player_.GetComponent<Savable>();
        if (Directory.Exists(scenePath))
        {
            string[] lines = File.ReadAllLines(scenePath);
            string data;
            for (int i = 0, id; i < lines.Length;)
            {
                id = ToInt32(lines[i]);
                data = "";
                for (i++; lines[i] != Savable.endObject; i++) data += lines[i];
                savables.Find((savable) => savable.id == id).Load(data);
                i++;
            }
            if (File.Exists(playerPath)) playerSavable.Load(File.ReadAllText(playerPath));
        }
    }

    public void AddDestroyedObjectSaveData(string data) => destroyedSaveData.Add(data);

    public bool TrySelectSaveInfo(int i)
    {
        try
        {
            saveFolder = saveFolders[i];
            currentSaveInfo = saveInfos[i];
            return true;
        }
        catch { return false; }
    }
    public int AddSaveInfo(SaveInfo info)
    {
        string savePath = Global.instance.savesPath + "\\" + info.name;
        Directory.CreateDirectory(savePath);
        File.WriteAllText(savePath + "\\save.info", info.MakeSaveData());
        saveFolders.Add(savePath);
        saveInfos.Add(info);
        UpdateGUIData();
        
        return saveInfos.Count - 1;
    }

    public bool SaveFolderExists(string name) => saveInfos.FindIndex((info) => info.name == name) >= 0;

    public event Action GUIDataChanged;

    private void UpdateGUIData()
    {
        GUIData guiData;
        SaveInfo info;
        GuiData = new List<GUIData>();
        for (int i = 0; i < saveInfos.Count; i++)
        {
            guiData = new GUIData();
            info = saveInfos[i];
            guiData.texts = new string[3];
            guiData.texts[0] = info.name;
            guiData.texts[1] = (info.playTime / 3600) + " hours";
            guiData.texts[2] = info.level + " level";
            GuiData.Add(guiData);
        }
        GUIDataChanged?.Invoke();
    }
}
