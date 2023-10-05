using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable 0649

public class GUIMainMenu : GUIObject
{
    private const string emptyNameError = "Name cannot be empty";
    private const string existingNameError = "Saved game with the same name already exists";

    [SerializeField] GUIEventButton newGameButton;
    [SerializeField] GUIEventButton loadGameButton;
    [SerializeField] GUIEventButton optionsButton;
    [SerializeField] GUIEventButton quitButton;
    [SerializeField] GUIEventButton[] otherButtons;


    [SerializeField] GUIContainer saveSlots;


    [SerializeField] GUIObject characterCreationWindow;

    [SerializeField] string defaultSceneName;

    [SerializeField] GUIObject nameEnterErrorMessage;
    [SerializeField] GUITextField nameEnterField;
    [SerializeField] GUIEventButton nameEnterConfirmButton;

    protected override void AwakeCustom()  { foreach (GUIEventButton obj in otherButtons) obj.Show(); }

    protected virtual void Start()
    {
        saveSlots.ItemInteraction += LoadManager.instance.LoadSave;
        nameEnterConfirmButton.Interaction += CheckNameField;
    }

    protected virtual void CheckNameField(GUIInteractiveObject obj)
    {
        string text = nameEnterField.text;
        if (text == "") nameEnterErrorMessage.SetTextAll(emptyNameError);
        else if (LoadManager.instance.SaveFolderExists(text)) nameEnterErrorMessage.SetTextAll(existingNameError);
        else CreateCharacter(text);
    }
    protected void CreateCharacter(string name)
    {
        int saveSlot = LoadManager.instance.AddSaveInfo(new SaveInfo(name, defaultSceneName, 0, 1));
        LoadManager.instance.LoadSave(saveSlot);
    }

    public override void Show() { }
    public override void Hide() { }
}
