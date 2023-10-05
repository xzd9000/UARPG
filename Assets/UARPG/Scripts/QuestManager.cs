using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class QuestManager : Singleton<QuestManager>
{
    [SerializeField] List<Quest> quests;
    [SerializeField] float positionCheckPeriod = 0.5f;

    private Player player;

    protected override void AwakeCustom()
    {
        player = Global.player;
        if (player == null)
        {
            Debug.LogWarning("Player was not found in the scene");
            return;
        }
        StartCoroutine(PositionCheck());
    }

    public void AddQuest(Quest quest) { if (!quests.Contains(quest)) quests.Add(quest); }

    public void NotifyDeath(Character dead) => Notify(dead);
    public void NotyifyInteraction(InteractiveObject obj) => Notify(obj);
    public void NotifyInventory(IInventoryItem item) => Notify(item);
    private void Notify(object obj)
    {
        for (int i = 0; i < quests.Count; i++)
        {
            if (obj is Character character) quests[i].CheckObject(character);
            else if (obj is InteractiveObject obj_) quests[i].CheckObject(obj_);
            else if (obj is IInventoryItem data) quests[i].CheckObject(data);
        }
    }
    
    private IEnumerator PositionCheck()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(positionCheckPeriod);
            for (int i = 0; i < quests.Count; i++) quests[i].CheckObject(player.transform.position);
        }
    }
}
