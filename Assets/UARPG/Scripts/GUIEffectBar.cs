using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public class GUIEffectBar : GUIObject
{
    private Effect[] effects;
    private GameObject[] effectBoxes;
    private int iconIndex = 0;

    protected override void AwakeCustom()
    {
        effectBoxes = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) effectBoxes[i] = transform.GetChild(i).gameObject;      
    }

    private void OnEnable() => player.EffectChange += UpdateEffects;
    private void OnDisable() => player.EffectChange -= UpdateEffects;             

    private void UpdateEffects(object sender, System.EventArgs args)
    {
        effects = player.GetEffects();
        iconIndex = 0;
        foreach (GameObject obj in effectBoxes)
        {
            obj.GetComponent<Image>().enabled = false;
            obj.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
        foreach (Effect effect in effects)
        {
            if (effect.icon != null)
            {
                if (iconIndex < effectBoxes.Length)
                {
                    GameObject box = effectBoxes[iconIndex];
                    RectTransform boxRect = box.GetComponent<RectTransform>();
                    box.GetComponent<Image>().enabled = true;
                    box.GetComponent<Image>().sprite = effect.icon;
                    if (effect.duration > 0)
                    {
                        box.transform.GetChild(0).GetComponent<Image>().enabled = true;
                        StartCoroutine(EffectTimer(box.transform.GetChild(0).gameObject, effect));
                    }
                    iconIndex++;
                }
            }
        }
    }
    private IEnumerator EffectTimer(GameObject obj, Effect effect)
    {
        while (effect.timeLeft > 0 && obj != null)
        {
            obj.GetComponent<RectTransform>().localScale = new Vector3(effect.timeLeft / effect.duration, 1, 1);
            yield return new WaitForEndOfFrame();
        }
    }
}
