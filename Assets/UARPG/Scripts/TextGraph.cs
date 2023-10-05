using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextGraph
{
    private List<string> texts = new List<string>();
    private List<Tuple<int, int>> connections = new List<Tuple<int, int>>();

    public int textsLength => texts.Count;
    public int connectionsLength => connections.Count;

    public void Add(string text, int index)
    {
        if (index >= texts.Count) texts.AddRange(new string[index - texts.Count + 1]);
        texts[index] = text;
    }
    public void Connect(int origin, params int[] connections)
    {
        for (int i = 0; i < connections.Length; i++) this.connections.Add(new Tuple<int, int>(origin, connections[i]));
    }

    public int[] GetConnections(int origin)
    {
        List<Tuple<int, int>> matches = connections.FindAll((match) => match.Item1 == origin);
        int[] ret = new int[matches.Count];
        for (int i = 0; i < ret.Length; i++) ret[i] = matches[i].Item2;
        return ret;
    }
    public List<string> GetTextList() => texts;

    public string this[int i] => texts[i];
}
