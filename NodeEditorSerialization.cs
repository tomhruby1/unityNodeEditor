using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//ADDITIONAL DATA TO SERIALIZE in levelDAG
//used for rebuilding graph in editor

[System.Serializable]
public class NodeData
{
    public int id;  //in levelDAG idx in array
    public string name;
    public string text;
    public float x;
    public float y;
    public float width;
    public float heigth;
}


