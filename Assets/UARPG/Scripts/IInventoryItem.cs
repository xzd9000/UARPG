using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IInventoryItem : IEquatable<IInventoryItem>
{
    int id { get; }
    string UIName { get; }
    Sprite icon { get; }
    string resourcePath { get; }
    Color rarityColor { get; }
    string description { get; }
    string extraText { get; }

    IInventoryItem CreateInstance();
    IInventoryItem Load();
    void Destroy();
}

