using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class NodeCondition
{
    public int id;
    public BasicNode node;   //parent node
    public GUIStyle style;
    public string condition;
    public ConnectionPoint outPoint;

    Action<NodeCondition> OnNodeCondUpdate;

    public int termCount = 1;

    public List<int> selected = new List<int>();    //selected option
    //TODO: should not be hardcoded - get levelState variables offline from SO
    public string[] options = LevelState.instance.levelVariables.ToArray();/* new string[4] { 
            "Generator", "Cable", "Cable2","Light" };*/    
    
    public List<string> value = new List<string>();

    public List <int> operatorSelected = new List<int>();
    public string[] operatorOptions = new string[2] { "is", "not"};

    public float y;    //cond offset inside node
    /*
    float x;
    float y;
    float width;
    float height;
    */
    public NodeCondition(int id, BasicNode node, GUIStyle nodeCondStyle, Action<ConnectionPoint> OnClickOutPoint, GUIStyle outPointStyle, Action<NodeCondition> OnNodeCondUpdate)
    {
        this.id = id;
        this.condition = "Default condition";
        this.node = node;
        this.style = nodeCondStyle;
        this.outPoint = new ConnectionPoint(this, this.node, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        this.OnNodeCondUpdate = OnNodeCondUpdate;
    }

    public void Draw()
    {
        //options = new List<string>(LevelState.instance.state.Keys).ToArray();

        this.y = node.rect.y + BasicNode.conditionOffset * (id + 1) + node.nodeHeaderHeight;
        
        //GUI.Box(new Rect(node.rect.x + 60, y, node.rect.width * 0.6f, 50), condition, style);
        //GUILayout.BeginArea(new Rect(node.rect.x + 60, y, node.rect.width * 0.6f, 100));
        
        EditorGUI.BeginChangeCheck();

        float rowX = node.rect.x + 10;
        float operatorWidth = 40;
        float popupWidth = 100;
        float valueWidth = 30;

        string condition = "";
        for(int i = 0; i < termCount; i++)
        {
            selected.Add(0);    //defaults
            operatorSelected.Add(0);
            value.Add("");

            //generate condition row 
            selected[i] = EditorGUI.Popup(new Rect(rowX + i * (Node.termOffset), y, popupWidth, 50), selected[i], options);       
            operatorSelected[i] = EditorGUI.Popup(new Rect(rowX + popupWidth + i * (Node.termOffset), y, operatorWidth, 20), operatorSelected[i], operatorOptions);
            value[i] = GUI.TextField(new Rect(rowX + popupWidth + operatorWidth + i * (Node.termOffset), y, valueWidth, 20), value[i], 3);
            //if last show "and" button
            if (i == termCount - 1)
            {
                if (GUI.Button(new Rect(rowX + popupWidth + operatorWidth + valueWidth + 5 + i * (Node.termOffset), y, 35, 20), "and"))
                {
                    termCount++;
                }
            }
            else
            {
                GUI.Label(new Rect(rowX + popupWidth + operatorWidth + valueWidth + 5 + i * (Node.termOffset), y, 35, 20), "and");
            }

            condition += options[selected[i]] + " " + operatorOptions[operatorSelected[i]] + " " + value[i];
            if (i < termCount - 1)
                condition += /*" and "*/" && ";
        }
        this.condition = condition;

        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("condition changed");
            OnNodeCondUpdate(this);
        }
       
        //GUILayout.EndArea();
        
    }
}

public class NodeAction
{
    public Action action;
    public int id;
    public BasicNode node;
    private float y;    //offset from top by id * const
    
    public NodeAction(int id, BasicNode node)
    {
        this.id = id;
        this.node = node;
    }
    
    public void Draw()
    {
        EditorGUI.BeginChangeCheck();

        y = node.rect.y + BasicNode.conditionOffset/2 * (id + 1);
        
        Rect position = new Rect(node.rect.x - node.actionRectWidth + 10, y, node.actionRectWidth - 20, 20);

        action = (Action)EditorGUI.ObjectField(position, action, typeof(Action), true);
       

        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("action changed");
            node.OnNodeActionUpdate(this);
        }
    }

    public void AddAction()
    {

    }
    
}
