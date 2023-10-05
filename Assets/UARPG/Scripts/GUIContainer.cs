using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public class GUIContainer : GUIObject
{
    private const string interfaceErrorMessage = "Assigned gui data list component is not IGUIDataList";

    [SerializeField][EditorReadOnly] int selectionIndex = -1;
    [SerializeField] bool grid;
    [SerializeField] bool fillManually;
    [SerializeField][HideUnless("fillManually", false)] GameObject baseElement;
    [SerializeField][HideUnless("fillManually", true)] bool formFromChildren;
    [SerializeField] Color selectionColor;
    [SerializeField] Color normalColor;
    [SerializeField] Color emptyColor;
    [SerializeField] GUIScrollBar scroll;
    [SerializeField] float indent;
    [SerializeField] bool useTooltips;
    [SerializeField][HideUnless("useTooltips", true)] TooltipFormatter tooltipFormatter;
    [SerializeField] List<GUIContainerEventButton> Items;

    [SerializeField] MonoBehaviour guiDataList;

    private IGUIDataList iGuiDataList;

    private int startingIndex = 0;
    private int count = 1;
    private int vCount = 1;

    public int totalCount { get; private set; }

    public List<GUIContainerEventButton> items => Items;

    private bool scrollExists;

    protected override void AwakeCustom()
    {
        scrollExists = scroll != null;
        if (guiDataList != null)
        {
            iGuiDataList = guiDataList as IGUIDataList;
            if (iGuiDataList == null) throw new NotSupportedException(interfaceErrorMessage);          
        }

        if (!fillManually)
        {
            RectTransform rect = GetComponentsInChildren<RectTransform>()[1];
            GameObject obj;
            if (rect == null)
            {
                if (baseElement != null)
                {
                    obj = Instantiate(baseElement, transform);
                    obj.transform.position = new Vector3(indent, indent);
                }
                else throw new MissingReferenceException(baseElementExceptionMessage);
            }
            else
            {
                obj = rect.gameObject;
                indent = rect.anchoredPosition.x;
            }

            if (obj.GetComponent<GUIContainerEventButton>() == null) obj.AddComponent<GUIContainerEventButton>();
            if (useTooltips && obj.GetComponent<TooltipSource>() == null) obj.AddComponent<TooltipSource>();
            
            count = (int)((this.rect.sizeDelta.y - indent) / (rect.sizeDelta.y + indent));
            if (count <= 0) count = 1;
            if (grid) vCount = (int)((this.rect.sizeDelta.x - indent) / (rect.sizeDelta.x + indent));            
            for (int i = 0; i < count; i++)
            {
                GameObject inst;
                GUIContainerEventButton button;
                if (grid)
                {
                    for (int ii = 0; ii < vCount; ii++)
                    {
                        instantiate(vCount * i + ii);
                        inst.GetComponent<RectTransform>().anchoredPosition = new Vector3(indent + ii * (rect.sizeDelta.x + indent), -(indent + i * (rect.sizeDelta.y + indent)));
                    }
                }
                else
                {
                    instantiate(i);
                    inst.GetComponent<RectTransform>().anchoredPosition = new Vector3(indent, -(indent + i * (rect.sizeDelta.y + indent)));
                }
                void instantiate(int index)
                {
                    inst = Instantiate(obj, transform);
                    button = inst.GetComponent<GUIContainerEventButton>();
                    button.SetColorAll(emptyColor);
                    button.Interaction += ChangeSelection;
                    button.InitIndex(index);
                    Items.Add(button);

                    if (useTooltips) button.GetComponent<TooltipSource>().Init(button, button, tooltipFormatter);
                    if (iGuiDataList != null) button.guiData = iGuiDataList.guiData[index];                    
                }
            }
            Destroy(obj);

            totalCount = count * vCount;
        }
        else
        {
            if (formFromChildren)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).TryGetComponent(out GUIContainerEventButton button))
                    {
                        Items.Add(button);
                        button.InitIndex(i);
                        button.Interaction += ChangeSelection;
                        button.SetColorAll(emptyColor);

                        if (iGuiDataList != null) button.guiData = iGuiDataList.guiData[i];
                        if (useTooltips) button.GetComponent<TooltipSource>().Init(button, button, tooltipFormatter);
                    }
                }
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].InitIndex(i);
                    items[i].Interaction += ChangeSelection;
                    if (iGuiDataList != null) items[i].guiData = iGuiDataList.guiData[i];
                    if (useTooltips) items[i].GetComponent<TooltipSource>().Init(items[i], items[i], tooltipFormatter);
                }
            }

            totalCount = Items.Count;
        }

        selectionIndex = -1;
        UpdateColors();
    }

    protected virtual void OnEnable()
    {
        if (iGuiDataList != null)
        {
            if (scrollExists) scroll.Scroll += UpdateDataVisibility;
            iGuiDataList.GUIDataChanged += UpdateDataVisibility;
            ShowGuiData(0);
        }
        control.ConfirmButtonPressed += InteractWithSelected;
        control.GuiMovement += ChangeSelection;
        control.CancelButtonPressed += ResetSelection;
        SelectionChange += UpdateColors;
    }
    protected virtual void OnDisable()
    {
        if (scrollExists) scroll.Scroll -= UpdateDataVisibility;
        if (iGuiDataList != null) iGuiDataList.GUIDataChanged -= UpdateDataVisibility;
        control.ConfirmButtonPressed -= InteractWithSelected;
        control.GuiMovement -= ChangeSelection;
        control.CancelButtonPressed -= ResetSelection;
    }

    public void ChangeGUIDataSource(IGUIDataList guiData)
    {
        OnDisable();
        iGuiDataList = guiData;
        OnEnable();
    }

    public event Action<int> SelectionChange;
    public event Action<int> ItemInteraction;

    private void ChangeSelection(MovementDirection direction)
    {
        //int newIndex = selectionIndex;
        //switch (direction)
        //{
        //    case MovementDirection.forward: newIndex -= vCount; break;
        //    case MovementDirection.backwards: newIndex += vCount; break;
        //    case MovementDirection.rigth: newIndex++; break;
        //    case MovementDirection.left: newIndex--; break;
        //    default: throw new IndexOutOfRangeException();
        //}
        //ChangeSelection(newIndex);
    }
    private void ChangeSelection(int selection)
    {
        int oldIndex = selectionIndex;
        if (selection < 0)
        {
            if (iGuiDataList != null)
            {
                startingIndex--;
                if (startingIndex < 0)
                {
                    selectionIndex = count - 1;
                    startingIndex = iGuiDataList.guiData.Count - count;
                }
                else selectionIndex = 0;
            }
            else selectionIndex = totalCount - 1;
        }
        else if (selection >= totalCount)
        {
            if (iGuiDataList != null)
            {
                startingIndex++;
                if (startingIndex + count - 1 >= iGuiDataList.guiData.Count)
                {
                    selectionIndex = 0;
                    startingIndex = 0;
                }
                else selectionIndex = totalCount - 1;
            }
            else selectionIndex = 0;
        }
        else selectionIndex = selection;
        SelectionChange?.Invoke(selectionIndex + startingIndex);
        if (scrollExists)
        {
            if (iGuiDataList != null)
            {
                if (iGuiDataList.guiData.Count > totalCount) scroll.SetPosition(selectionIndex + startingIndex, iGuiDataList.guiData.Count);
            }
        }
        ShowGuiData(startingIndex);
    }
    private void ChangeSelection(GUIInteractiveObject obj)
    {
        if (obj is GUIContainerEventButton button)
        {
            int index = button.index;
            ChangeSelection(index);           
            InteractWithSelected();
        }
    }
    public void ResetSelection()
    {
        if (selectionIndex != -1)
        {
            selectionIndex = -1;
            SelectionChange?.Invoke(-1);
        }
    }
    private void InteractWithSelected() { if (selectionIndex != -1) ItemInteraction?.Invoke(selectionIndex + startingIndex); }

    private void UpdateColors(int i) => UpdateColors();
    public void UpdateColors()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i == selectionIndex) items[i].SetColorAll(selectionColor);                      
            else if (items[i].hasImages) items[i].SetColorAll(normalColor);           
            else items[i].SetColorAll(emptyColor);
        }
    }

    private void UpdateDataVisibility() => UpdateDataVisibility(0f);
    private void UpdateDataVisibility(float scrollPosition)
    {
        int tIndex = (int)(Mathf.Clamp(iGuiDataList.guiData.Count - totalCount + 1, 0, float.PositiveInfinity) * scrollPosition) * vCount;
        ShowGuiData(tIndex);
        if (tIndex != startingIndex) startingIndex = tIndex;       
    }

    private void ShowGuiData(int startingIndex)
    {
        if (iGuiDataList != null)
        {
            List<GUIData> guiData = iGuiDataList.guiData;
            int dataLength = guiData.Count;
            for (int listIndex = 0, dataIndex = startingIndex; listIndex < items.Count; listIndex++, dataIndex++)
            {
                GUIContainerEventButton obj = items[listIndex];
                if (dataIndex < dataLength)
                {
                    GUIData data = guiData[dataIndex];
                    obj.SetImages(data.images);
                    obj.SetTexts(data.texts);
                    obj.guiData = data;
                }
                else
                {
                    obj.SetImageAll(null);
                    obj.SetTextAll("");
                    obj.guiData = new GUIData();
                }
            }
        }
        UpdateColors();
    }

}
