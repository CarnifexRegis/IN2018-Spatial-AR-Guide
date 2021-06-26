using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnchorStore
{
    public string name;
    public Anchor anchor;
}

[System.Serializable]
public class Anchor
{
    public string name;
    public string id;
    public string text;
    public List<Anchor> children;
}