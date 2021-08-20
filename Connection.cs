using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The ConnectionPoint class allows us to define our points to connect different nodes
/// </summary>
public enum ConnectionPointType { In, Out }
public class ConnectionPoint
{
    public Rect rect;

    public ConnectionPointType type;

    public Node node;

    public GUIStyle style;

    public Action<ConnectionPoint> OnClickConnectionPoint;

    public NodeCondition condition = null;

    public ConnectionPoint(Node node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }
    
    public ConnectionPoint(NodeCondition nodeCondition, Node node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.condition = nodeCondition;
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }
    //Different draw for OutPoint and InPoint --type
    public void DrawIn()
    {
        rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

        if (node.GetType() == typeof(BasicNode)) //Basic nodes has actions rect
        {
            BasicNode bNode = (BasicNode)node;
            rect.x = node.rect.x - rect.width + 8f - bNode.actionRectWidth;
        } 
        else
            rect.x = node.rect.x - rect.width + 8f;

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
    public void DrawOut()
    {
        rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;
        rect.x = node.rect.x + node.rect.width - 8f;

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
    /// <summary>
    /// Used with conditions => BasicNode
    /// </summary>
    /// <param name="cond"></param>
    public void DrawOut(NodeCondition cond)
    {
        rect.y = cond.y;
        rect.x = node.rect.x + node.rect.width - 8f;

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
}



public class Connection
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    public Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw()
    {
        Handles.DrawBezier(
            inPoint.rect.center,
            outPoint.rect.center,
            inPoint.rect.center + Vector2.left * 50f,
            outPoint.rect.center - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );

        if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleCap))
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }
    }
}