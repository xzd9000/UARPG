using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GUILocalizedText : MonoBehaviour
{
    [SerializeField] LocalizedTextBlock localizedText;

    private Text text;

    private void Awake() => text.text = localizedText.text;
}
