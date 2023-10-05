using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Convert;

#pragma warning disable 0649

public class DialogText : MonoBehaviour, IGUIDataList
{
    private int activeEntry;
    private int[] availableEntries;

    [SerializeField] LocalizedTextBlock textSource;
    [SerializeField] List<string> lines;
    [SerializeField] List<int> currentEntryIndexes;
    [SerializeField] List<GUIData> GuiData;

    private TextGraph dialogText;

    public List<GUIData> guiData => GuiData;

    private void Awake()
    {
        ParseText();
        SelectEntry(0);
        lines = dialogText.GetTextList();
    }

    private void ParseText()
    {
        if (textSource != null)
        {
            string text = textSource.text;
            string temp = "";
            string[] numsStr;
            int[] nums;
            int index = -1;
            for (int i = 0; i < text.Length;)
            {               
                addUntil('#');
                index = ToInt32(temp);
                addUntil('|');
                dialogText.Add(temp, index);
                addUntil('\n');

                numsStr = temp.Split(',');
                nums = new int[numsStr.Length];

                for (int ii = 0; ii < numsStr.Length; ii++) nums[ii] = ToInt32(numsStr[ii]);
                dialogText.Connect(index, nums);

                void addUntil(char stop)
                {
                    temp = "";
                    while (text[i] != stop)
                    {
                        temp += text[i];
                        i++;
                    }
                }
            }
        }
    }

    public void SelectEntry(int index)
    {
        if (availableEntries.Contains(index))
        {
            activeEntry = index;
            availableEntries = dialogText.GetConnections(index);
            GuiData = new List<GUIData>();
            GUIData data = new GUIData();
            data.texts = new string[1];
            data.texts[0] = dialogText[activeEntry];

            for (int i = 0; i < availableEntries.Length; i++)
            {
                data = new GUIData();
                data.texts[0] = (i + 1).ToString();
                data.texts[1] = dialogText[availableEntries[i]];
            }

            GUIDataChanged?.Invoke();
        }
    }

    public event System.Action GUIDataChanged;
}
