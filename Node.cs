using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public abstract class Node
{
    public int id;
    
    public Rect rect;   //node body rect
    public float baseRectWidth;
    
    public string name;
    public string description = "node description";

    public bool isDragged;
    public bool isSelected;

    public ConnectionPoint inPoint;
    //public ConnectionPoint outPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Action<Node> OnRemoveNode;

    public float nodeHeaderHeight = 80;
    protected GUIStyle nodeTitleStyle = new GUIStyle();

    protected GUIStyle inPointStyle;
    protected GUIStyle outPointStyle;

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public const float termOffset = 205;   //offset between terms --ands

    /// <summary>
    /// basic implementation of Draw func, 
    /// </summary>
    public virtual void Draw()
    {
        //node body
        GUI.Box(rect, "", style);
        //node title
        name = GUI.TextField(new Rect(rect.x + rect.width / 4, rect.y + 2.5f, rect.width / 2, 20), name, 10, nodeTitleStyle);
    }
    
    public virtual bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    protected void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }

    protected virtual void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }
}

public class BasicNode : Node
{
    public List<NodeCondition> outConditions;
    public List<NodeAction> actions;    //actions when transition to current node

    public Rect actionsRect;
    public float actionRectWidth = 200f;    //node actions list width -- meh probably should not be here

    //parementers for NodeCondtion
    private GUIStyle nodeConditionStyle;
    private Action<ConnectionPoint> OnClickOutPoint;

    public const int conditionOffset = 50;

    public Action<NodeCondition> OnNodeCondUpdate;
    public Action<NodeAction> OnNodeActionUpdate;

    public string[] levelComponentNames;    //TODO: move upstream
    public BasicNode(int id, Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle nodeConditionStyle,
        GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, 
        Action<Node> OnClickRemoveNode, 
        Action<NodeCondition> OnNodeCondUpdate, Action<NodeAction> OnNodeActionUpdate)
    {
        this.id = id;
        this.name = "node " + id.ToString();
        rect = new Rect(position.x, position.y, width, height);
        baseRectWidth = width;
        style = nodeStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        //outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        this.nodeConditionStyle = nodeConditionStyle;
        this.outPointStyle = outPointStyle;
        this.OnClickOutPoint = OnClickOutPoint;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;
        this.OnNodeActionUpdate = OnNodeActionUpdate;
        this.OnNodeCondUpdate = OnNodeCondUpdate;

        outConditions = new List<NodeCondition>();
        actions = new List<NodeAction>();
        actions.Add(new NodeAction(0, this));

        nodeTitleStyle.alignment = TextAnchor.MiddleCenter;
        nodeTitleStyle.fontStyle = FontStyle.Bold;
        nodeTitleStyle.normal.textColor = Color.white;

        actionsRect = new Rect(rect.x - actionRectWidth, rect.y, actionRectWidth, rect.height);

    }

    public override void Draw()
    {
        //rewrite componentNames to currently available LevelState variables for conditions 
        //levelComponentNames = new List<string>(LevelState.instance.state.Keys).ToArray();   

        rect.height = outConditions.Count * conditionOffset + nodeHeaderHeight + conditionOffset;

        inPoint.DrawIn();
        //actions box
        actionsRect.x = rect.x - actionRectWidth;
        actionsRect.y = rect.y;
        actionsRect.height = rect.height;
        GUI.Box(actionsRect, "", style);

        foreach (NodeAction action in actions)
        {
            action.Draw();
        }

        //edit node according to max number of terms in conds
        int maxTerms = 1;
        foreach (NodeCondition cond in outConditions)
        {
            if (cond.termCount > maxTerms)
                maxTerms = cond.termCount;
        }

        rect.width = maxTerms * termOffset + 30;
        
        //node body
        GUI.Box(rect, id.ToString(), style);   
        //node title
        name = GUI.TextField(new Rect(rect.x + rect.width / 4, rect.y + 2.5f, rect.width / 2, 20), name, 10, nodeTitleStyle);
        //descr.
        description = GUI.TextArea(new Rect(rect.x + rect.width / 8, rect.y + 20, rect.width / 8 * 6.5f, nodeHeaderHeight - 10), description, 200);

        foreach (NodeCondition cond in outConditions)
        {
            cond.outPoint.DrawOut(cond);
            cond.Draw();
        }
    }

    private void AddNodeCondition()
    {
        outConditions.Add(new NodeCondition(outConditions.Count, this, nodeConditionStyle, OnClickOutPoint, outPointStyle, OnNodeCondUpdate));
    }

    public void AddAction()
    {
        actions.Add(new NodeAction(actions.Count, this));
    }

    protected override void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        //genericMenu.AddItem(new GUIContent("Set title"), false, SetNodeTitle);
        genericMenu.AddItem(new GUIContent("Add condition"), false, AddNodeCondition);
        genericMenu.AddItem(new GUIContent("Add action"), false, AddAction);
        genericMenu.ShowAsContext();
    }

    /// <summary>
    /// Overridden for checking mouse events on actionsRect also
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition) || actionsRect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition) || e.button == 1 && isSelected && actionsRect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }
}

public class StartNode : Node
{
    public ConnectionPoint outPoint;
    private Action<ConnectionPoint> OnClickOutPoint;
    private float fieldRectWidth = 200f;

    //private static LevelState instance = null; TODO sigleton

    public StartNode(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle,GUIStyle outPointStyle, 
        Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode)
    {
        id = 0;
        this.name = "start ";
        rect = new Rect(position.x, position.y, width, height);
        baseRectWidth = width;
        style = nodeStyle;
  
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        this.outPointStyle = outPointStyle;
        this.OnClickOutPoint = OnClickOutPoint;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;

        nodeTitleStyle.alignment = TextAnchor.MiddleCenter;
        nodeTitleStyle.fontStyle = FontStyle.Bold;
        nodeTitleStyle.normal.textColor = Color.white;

    }
    StartNode()
    {
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
    }

    public override void Draw()
    {
        //node body
        GUI.Box(rect, "", style);
        //node title
        GUI.Label(new Rect(rect.x + rect.width / 4, rect.y + 2.5f, rect.width / 2, 20), name, nodeTitleStyle);
        //Load level state for state var options
        GUI.Label(new Rect(rect.x + 10f, rect.y + 35f, rect.width / 2, 20), "LevelState:");
        Rect position = new Rect(rect.x + 80f, rect.y + 35f, fieldRectWidth - 55, 20);
        NodeEditor.state = (LevelState)EditorGUI.ObjectField(position, NodeEditor.state, typeof(LevelState), true);
        //Load LevelDag
        GUI.Label(new Rect(rect.x + 10f, rect.y + 60f, rect.width / 2, 20), "LevelDAG:");
        position = new Rect(rect.x + 80f, rect.y + 60f, fieldRectWidth - 55, 20);
        NodeEditor.state = (LevelState)EditorGUI.ObjectField(position, NodeEditor.state, typeof(LevelState), true);

        outPoint.DrawOut();

    }
}




/*
[System.Serializable]
public class NodeData
{

    public int id_Node;
    public Vector2 position;
}

[System.Serializable]
public class NodeDataCollection
{
    public NodeData[] nodeDataCollection;
}
*/

/*
        int selGridInt = 0;
        string[] selStrings = new string[] { "Grid 1", "Grid 2", "Grid 3", "Grid 4" };
        selGridInt = GUI.SelectionGrid(new Rect(25, 25, 100, 30), selGridInt, selStrings, 2);
        */

//toolbar?
/*
int toolbarInt = 0;
string[] toolbarStrings = new string[] { "Toolbar1", "Toolbar2", "Toolbar3" };
toolbarInt = GUI.Toolbar(new Rect(25, 25, 250, 30), toolbarInt, toolbarStrings);
*/
