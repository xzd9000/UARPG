using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public class BossHPBars : Singleton<BossHPBars>
{

    [SerializeField] GameObject bossHPBar;
    [SerializeField] List<Character> bosses;
    private Dictionary<Character, GameObject> bossHPBars;

    protected override void AwakeCustom()
    {
        if (bossHPBar == null) bossHPBar = Resources.Load<GameObject>("UI/BossHPBar");
        if (bossHPBars == null) bossHPBars = new Dictionary<Character, GameObject>();
    }

    public void AddBoss(Character character)
    {
        if (character.GetComponent<BossAI>() == null) Debug.LogWarning("Added character is not a boss");
        else
        {
            bosses.Add(character);
            GameObject obj = Instantiate(bossHPBar,transform);
            obj.GetComponentInChildren<Text>().text = character.UIName;
            bossHPBars.Add(character, obj);
            character.HealthChange += UpdateBar;
            character.Death += RemoveBoss;
            UpdateHPBars();
        }
    }
    public void RemoveBoss(object sender, System.EventArgs args)
    {
        if (sender is Character character)
        {
            character.HealthChange -= UpdateBar;
            character.Death -= RemoveBoss;
            GameObject obj = bossHPBars[character];
            bossHPBars.Remove(character);
            bosses.Remove(character);
            Destroy(obj);
            UpdateHPBars();
        }
    }
    private void UpdateBar(object sender, System.EventArgs args)
    {
        if (sender is Character character)
        {
            if (bossHPBars.ContainsKey(character))
            {
                RectTransform rcTrans = bossHPBars[character].GetComponentsInChildren<Image>()[1].GetComponent<RectTransform>();
                rcTrans.localScale = new Vector3((character.health / character.stats.values[character.maxHealthStatIndex].value), 1, 1);
            }
        }
    }
    private void UpdateHPBars()
    {
        for (int i = 0; i < bosses.Count; i++)
        {
            RectTransform rcTrans = bossHPBars[bosses[i]].GetComponent<RectTransform>();
            rcTrans.anchoredPosition = new Vector2(0, -rcTrans.sizeDelta.y * (i + 1));
        }

    }
}
