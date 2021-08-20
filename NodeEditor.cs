using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Text;

public class NodeEditor : EditorWindow, ISerializationCallbackReceiver
{
    public List<Node> nodes;
    public List<Connection> connections;

    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle nodeConditionStyle;
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;

    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;

    private Vector2 offset;
    private Vector2 drag;


    public LevelDAG levelDAG;

    [MenuItem("Window/Node Editor")]
    private static void OpenWindow()
    {
        NodeEditor window = GetWindow<NodeEditor>();
        window.titleContent = new GUIContent("Node Editor");
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        nodeConditionStyle = new GUIStyle();
        nodeConditionStyle.normal.background = EditorGUIUtility.Load("green") as Texture2D;
        nodeConditionStyle.border = new RectOffset(12, 12, 12, 12);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);

        AddStartNode(new Vector2(0, 0));
    }

    private Vector2 scale = new Vector2(0.5f, 0.5f);
    private Vector2 pivotPoint;

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawNodes();
        DrawConnections();

        DrawConnectionLine(Event.current);

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        /*       
        pivotPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        GUIUtility.ScaleAroundPivot(scale, pivotPoint);
        */
        if (GUI.changed) Repaint();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    ClearConnectionSelection();
                }

                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private void ResetDAG()
    {
        nodes = new List<Node>();
        connections = new List<Connection>();
        AddStartNode(new Vector2(0, 0));
    }    

    //ZOOM
    private void ZoomOut()
    {
        float zoomScale = 0.5f;
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
        GUI.matrix = /*translation * */scale /* * translation.inverse*/;

        Debug.Log(GUI.matrix);
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
        //genericMenu.AddItem(new GUIContent("Add start node"), false, () => OnClickAddStartNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Save"), false, () => SaveDAG());
        genericMenu.AddItem(new GUIContent("Load"), false, () => LoadDAG());
        genericMenu.AddItem(new GUIContent("Zoom out"), false, () => ZoomOut());
        genericMenu.AddItem(new GUIContent("Reset"), false, () => ResetDAG());
        genericMenu.ShowAsContext();
    }

    private void LoadDAG()
    {
        Deserialize();
    }

    private void SaveDAG()
    {
        UpdateSerializedNodeData();
        levelDAG.Save();
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    /// <summary>
    /// Creating nodes
    /// </summary>
    /// <param name="mousePosition"></param>
    private void OnClickAddNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }

        int id = levelDAG.getFreeId();
        
        
        if(id < 0)
        {
            Debug.LogWarning("No free node-ID");
            return;
        }
        Debug.LogWarning("id assigned: " + id);

        levelDAG.AddNode(id);

        nodes.Add(new BasicNode(id, mousePosition, 250, 50, nodeStyle, selectedNodeStyle, nodeConditionStyle, inPointStyle,
            outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, UpdateDAGConditions, UpdateDAGActions));

        foreach (Node node in nodes)
            Debug.Log(node.name);
    }
   
    private void AddStartNode(Vector2 mousePosition) //todo automatically
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }

        nodes.Add(new StartNode(mousePosition, 250, 50, nodeStyle, selectedNodeStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
        levelDAG =  LevelDAG.CreateLevelDAG();
        EditorUtility.SetDirty(levelDAG);   //??
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickRemoveNode(Node node)
    {
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint /*|| connections[i].outPoint == node.outPoint*/)
                {
                    connectionsToRemove.Add(connections[i]);
                }

                if(node.GetType() == typeof(BasicNode)) 
                {
                    BasicNode bNode = (BasicNode)node;
                    foreach (NodeCondition outCond in bNode.outConditions)
                    {
                        if (connections[i].outPoint == outCond.outPoint)
                        {
                            connectionsToRemove.Add(connections[i]);
                        }
                    }
                }
                else if(node.GetType() == typeof(StartNode))
                {
                    StartNode sNode = (StartNode)node;
   
                    if (connections[i].outPoint == sNode.outPoint)
                    {
                        connectionsToRemove.Add(connections[i]);
                    }
                
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                levelDAG.RemoveEdge(connectionsToRemove[i].outPoint.node.id, connectionsToRemove[i].outPoint.condition.id);
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        levelDAG.RemoveNode(node.id);
        nodes.Remove(node);
        
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
        levelDAG.RemoveEdge(connection.outPoint.node.id, connection.outPoint.condition.id);
    }

    private void CreateConnection()     //TODO: osetrit -> nelze pridat, pokud jeden spoj uz existuje
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }

        //if connection from outPoint exists, remove
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].outPoint == selectedOutPoint)
            {
                OnClickRemoveConnection(connections[i]);
            }
        }
        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));

        //add edge to DAG cond or to empty cond (start node)
        if (selectedOutPoint.condition != null)
            levelDAG.AddEdge(selectedOutPoint.node.id, selectedInPoint.node.id, selectedOutPoint.condition.id, selectedOutPoint.condition.condition);
        else
            levelDAG.AddEdgeEmptyCond(selectedOutPoint.node.id, selectedInPoint.node.id);
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    public void UpdateDAGConditions(NodeCondition condition)   //hmmmm?
    {
        //levelDAG.UpdateConditon(condition.node.id, condition.)

        levelDAG.UpdateConditon(condition.node.id, condition.id, condition.condition);
        Debug.Log(condition.node.id.ToString() 
            + " node: updating conditions " + condition.id.ToString() + " " + condition.condition);
        
    }
    
    /// <summary>
    /// Load all actions on node and store to LevelDAG
    /// </summary>
    /// <param name="nodeAction"></param>
    public void UpdateDAGActions(NodeAction nodeAction)
    {
        List<Action> actions;
        List<string[]> lActions;    //levelAction encodes action with ID and 
        if (nodeAction.node.GetType() == typeof(BasicNode)) //Basic nodes has actions rect
        {
            BasicNode bNode = (BasicNode)nodeAction.node;
            actions = new List<Action>();   //list of references to MonoBeh. actions
            lActions = new List<string[]>();

            foreach (NodeAction nAction in bNode.actions)
            {
                actions.Add(nAction.action);
            }
            levelDAG.UpdateActions(nodeAction.node.id, actions.ToArray());
        }
    }

    
    public void UpdateSerializedNodeData()
    {
        //List<NodeData> nd = new List<NodeData>();
        NodeData[] nd = new NodeData[nodes.Count];
        for(int i = 0; i < nodes.Count; i++)
        {
            int id = nodes[i].id;
            //Debug.Log("Haha id: " + id);
            NodeData node = new NodeData();
            node.name = nodes[i].name;
            node.text = nodes[i].description;
            node.x = nodes[i].rect.x;
            node.y = nodes[i].rect.y;
            node.width = nodes[i].rect.width;
            node.heigth = nodes[i].rect.height;
            node.id = id;
            nd[id] = node;
        }
        levelDAG.nodeData = nd;
    }
    /// <summary>
    /// Reconstruction from json
    /// </summary>
    public void Deserialize()
    {
        levelDAG = LevelDAG.LoadFromJson(levelDAG);
        
        nodes = new List<Node>();   //reset lists
        connections = new List<Connection>();

        //recreate nodes
        foreach(NodeData node in levelDAG.nodeData)
        {
            Vector2 position = new Vector2(node.x, node.y);

            if (node.id == 0)   //start node
            {
                StartNode nn = new StartNode(position, node.width, node.heigth, nodeStyle, selectedNodeStyle, outPointStyle,
                                    OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
                nodes.Add(nn);
            }
            else
            {
                BasicNode nn = new BasicNode(node.id, position, node.width, node.heigth, nodeStyle, selectedNodeStyle, nodeConditionStyle,
                                    inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, UpdateDAGConditions,
                                    UpdateDAGActions);
                nn.description = node.text;
                nn.name = node.name;

                //actions reconstruction
                List<NodeAction> nodeActions = new List<NodeAction>();
                int actionID = 0; 
                foreach(LevelDAGNodeAction lAction in levelDAG.nodes[node.id].lActions)
                {
                    NodeAction nodeAction = new NodeAction(actionID, nn);
                    //locate Action monobehaviour for node editor visualization
                    GameObject actor = GameObject.Find(lAction.name);
                    Action[] availableActions = actor.GetComponents<Action>();
                    foreach (Action act in availableActions)
                    {
                        if (act.id == lAction.id)
                        {
                            nn.AddAction();
                            nodeAction.action = act;
                            break;
                        }
                    }
                    nn.actions.Add(nodeAction);
                    actionID++;
                }
                nodes.Add(nn);
            }

        }
        //make connections
        foreach (NodeData node in levelDAG.nodeData)
        {
            //search for relevant node in recreated list
            Node match = (Node)nodes.Find(item => item.id == node.id);

            if (node.id == 0)
            {
                StartNode start = (StartNode)match;
                Node next = nodes.Find(item => item.id == levelDAG.nodes[0].conds[0].child);
                //create connection, assign to outPoint created by NodeCondition constructor
                Connection conn = new Connection(next.inPoint, start.outPoint, OnClickRemoveConnection);
                connections.Add(conn);
                continue;
            }
            
            BasicNode current = (BasicNode)match;
            
            foreach (LevelDAGNodeCondition cond in levelDAG.nodes[node.id].conds)
            { 
                Node next = nodes.Find(item => item.id == cond.child);
                //add this condition to current node
                NodeCondition nc = new NodeCondition(cond.id, current, nodeConditionStyle,
                    OnClickOutPoint, outPointStyle, UpdateDAGConditions);
                current.outConditions.Add(nc);
                //create connection, assign to outPoint created by NodeCondition constructor
                Connection conn = new Connection(next.inPoint, nc.outPoint, OnClickRemoveConnection);
                connections.Add(conn);
                Debug.Log("Creating connection: " + conn.outPoint.node.id + " => " + conn.inPoint.node.id);

                //conditions reconstruction
                rebuildConditions(nc, cond);

                
            }

        }
    }


    /// <summary>
    /// Rebuild conditions from serializad condition expressions
    /// </summary>
    /// <returns></returns>
    void rebuildConditions(NodeCondition nc, LevelDAGNodeCondition cond)
    {
        string[] subexps = cond.condition.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string subexp in subexps)
        { 
            //Debug.Log(subexp + ":");
            bool condSatis = false;
            //single conjunction
            string[] terms = subexp.Split(new[] { " ", "&&" }, StringSplitOptions.RemoveEmptyEntries);     //chop into terms    
            List<bool> evalExp = new List<bool>();
            int i = 0;
            while (true)
            {
                //Debug.Log("terms: " + terms[i] /*+ terms[i + 1]*/);
                string stateVar = terms[i];
                string oper = terms[i + 1];
                string val = terms[i + 2];
                if (oper == "location")
                {

                }
                int stateVal = LevelState.instance.GetValue(stateVar);
                int ival = Int32.Parse(val);

                //setup selected stateVar
                nc.selected.Add(Array.IndexOf(nc.options, stateVar));

                //term eval
                if (oper == "is")
                {
                    nc.operatorSelected.Add(0);
                }
                else if (oper == "not")
                {
                    nc.operatorSelected.Add(1);
                }
                else
                {
                    Debug.Log("Nah, should not be. --> Error!");
                }

                //value
                nc.value.Add(val);
                
                i += 3;     //skip to another term -- add AND
                if (i >= terms.Length)  //or quit
                    break;
                nc.termCount++;

            }  
        }
    }


    // ISerializationCallbackReceiver functions
    // automatic ser. & deser. when starting playmode etc. 
    public void OnBeforeSerialize()
    {
      //  LoadDAG();

    }

    public void OnAfterDeserialize()
    {
       // SaveDAG();
    }



    /// <summary>
    /// NOT USED
    /// Returns list of connection lists -- sublisted if same targets
    /// </summary>
    /// <param name="connections"></param>
    /// <returns></returns>
    public static List<List<Connection>> MergeConnectionsByNodes(List<Connection> connections)
    {
        List<List<Connection>> compConnections = new List<List<Connection>>();
        List<Node> targetNodes = new List<Node>();
        for (int i = 0; i < connections.Count; i++)
        {
            if (targetNodes.Contains(connections[i].inPoint.node))  //inPoint node == target node (== "or")
            {
                int idx = targetNodes.FindIndex(el => el == connections[i].inPoint.node);   //position where already
                compConnections[idx].Add(connections[i]);   //add to corresponding position
            }
            else  //targetNode not yet used -> new condition
            {
                compConnections[i].Add(connections[i]);
                targetNodes.Add(connections[i].inPoint.node);
            }
        }
        return compConnections;
    }
}




