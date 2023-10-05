using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

[System.Serializable] public class Quest
{
    [SerializeField] LocalizedTextBlock UIName;
    [SerializeField] LocalizedTextBlock UIDescription;
    [SerializeField] QuestObjective[] objectives;
    [SerializeField] float positionCheckPeriod;
    [SerializeField] bool Completed;

    public bool completed => Completed;
    public QuestObjective[] GetObjectives() => objectives.Copy();

    public void CheckObject(Character target) => CheckObject(target, Objective.kill);
    public void CheckObject(IInventoryItem item) => CheckObject(item, Objective.obtain);
    public void CheckObject(Vector3 position) => CheckObject(position, Objective.move);
    public void CheckObject(InteractiveObject obj) => CheckObject(obj, Objective.interact);
    private void CheckObject(object obj, Objective objective)
    {
        for (int i = 0; i < objectives.Length; i++)
        {
            if (objectives[i].objective == objective)
            {       
                switch (objective)
                {
                    case Objective.kill: {
                            if (objectives[i].target.id == ((Character)obj).id) objectives[i].count.x++;
                            if (objectives[i].count.x >= objectives[i].count.y) CheckObjectives();
                            break;
                    }
                    case Objective.obtain: {
                            if ((objectives[i].item.item as IInventoryItem).Equals((IInventoryItem)obj))
                            {
                                InventoryItemData data;
                                if (Global.player.inventory.Find((IInventoryItem)obj, out data) != -1)
                                {
                                    objectives[i].count.x = data.count;
                                    if (objectives[i].count.x >= objectives[i].count.y) CheckObjectives();
                                }
                            }
                            break;
                    }
                    case Objective.move: {
                            if (Vector3.Distance((Vector3)obj, objectives[i].position) <= objectives[i].reachDistance)
                            {
                                objectives[i].count.x = 1;
                                CheckObjectives();
                            }
                            break;
                    }
                    case Objective.interact: {
                            if (((InteractiveObject)obj).gameObject == objectives[i].interactiveObject.gameObject)
                            {
                                objectives[i].count.x = 1;
                                CheckObjectives();
                            }
                            break;
                    }
                    default: throw new System.NotImplementedException();
                }
            }
        }
    }
    private void CheckObjectives()
    {
        bool completed = true;
        for (int i = 0; i < objectives.Length; i++)
        {
            if (objectives[i].count.x < objectives[i].count.y)
            {
                completed = false;
                break;
            }
        }
        Completed = completed;
    }
}
