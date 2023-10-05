public interface IGUIDataList
{
    System.Collections.Generic.List<GUIData> guiData { get; }
                         event System.Action GUIDataChanged;
}

public interface IGUIDataProvider { GUIData guiData { get; } }