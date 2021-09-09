using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;

[Serializable]
public class LevelDAGNodeCondition
{
    public int id;
    public string condition;
    public int child;

    public LevelDAGNodeCondition(int id, string condition, int childId)
    {
        this.id = id;
        this.condition = condition;
        this.child = childId;
    }
}
/// <summary>
/// Used for accessing MonoBehavoiour actions
/// TODO: maybe pass references? problems when multiple GOs of the same name
/// </summary>
[System.Serializable]
public class LevelDAGNodeAction
{
    public string id;   //Unique ID for each action inside one GameObject
    public string name; //Target GO. Unique name!!
    //public string 
    public LevelDAGNodeAction(string name, string id)
    {
        this.id = id;
        this.name = name;
    }
}

[System.Serializable]
public class LevelDAGNode
{
    public int id;

    //public Dictionary<int, LevelDAGNodeCondition> conds;    //pro autoserializaci je treba predelat
    //public LevelDAGNodeCondition[] conds;
    public List<LevelDAGNodeCondition> conds;
    //public Action[] actions;
    public List<LevelDAGNodeAction> lActions;

    public LevelDAGNode(int id)
    {
        //conds = new Dictionary<int, LevelDAGNodeCondition>();
        //conds = new LevelDAGNodeCondition[256];
        conds = new List<LevelDAGNodeCondition>();
        this.id = id;
    }

}

/// <summary>
/// Level states data structure
/// </summary>
[System.Serializable]
[CreateAssetMenu]
public class LevelDAG : ScriptableObject
{
    //Gameplay
    public LevelDAGNode[] nodes;
    //Additional data for editor, indexed by id
    public NodeData[] nodeData;
    
    /// <summary>
    ///     Instantiate wrapper, function like constructor.
    /// </summary>
    public static LevelDAG CreateLevelDAG()
    {
        LevelDAG level = ScriptableObject.CreateInstance<LevelDAG>();

        int MAX_NODES = 256;    
        level.nodes = new LevelDAGNode[MAX_NODES];
        level.nodes[0] = new LevelDAGNode(-2);    //-2 for start node -> 0 only for not initialized

        level.nodeData = new NodeData[MAX_NODES];

        Debug.Log("LevelDAG created");
        return level;
    }
    
    public void AddNode(int id)
    {
        nodes[id] = new LevelDAGNode(id);
        nodes[id].id = id;
        //TODO: increase size of nodes
        Log();
    }

    public void RemoveEdge(int id, int conditionId)
    {
        //nodes[id].conds.Remove(conditionId);
        //nodes[id].conds[conditionId] = null;
        for (int i = 0; i < nodes[id].conds.Count; i++)
        {
            if (nodes[id].conds[i].id == conditionId)
                nodes[id].conds.RemoveAt(i);
        }

    }

    public void RemoveNode(int id)
    {
        nodes[id] = null;
    }

    public void AddEdge(int parentId, int childId, int conditionId, string condition)
    {
        if (condition != null)
        {
            Debug.Log("Node " + parentId.ToString() + " AddedEdge: " + conditionId + ", " + condition);
            //nodes[parentId].conds.Add(conditionId, new LevelDAGNodeCondition(conditionId, condition, childId));
            //nodes[parentId].conds[conditionId] = new LevelDAGNodeCondition(conditionId, condition, childId);
            nodes[parentId].conds.Add(new LevelDAGNodeCondition(conditionId, condition, childId));
        }

        else//both exists -- should not happen
            Debug.Log("nah, that be BS bro");
    }

    public void AddEdgeEmptyCond(int parentId, int childId)
    {
        //nodes[parentId].conds.Add(0, new LevelDAGNodeCondition(parentId, "", childId));
        //nodes[parentId].conds[0] = new LevelDAGNodeCondition(parentId, "", childId);
        nodes[parentId].conds.Add(new LevelDAGNodeCondition(parentId, "", childId));
    }
    /// <summary>
    /// Update condition connecting given node towards childId
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="childId"></param>
    public void UpdateConditon(int nodeId, int conditionId, string newCondition)
    {
        //if (nodes[nodeId].conds[conditionId] != null)
        //{
        //    nodes[nodeId].conds[conditionId].condition = newCondition;
        //}
        for (int i = 0; i < nodes[nodeId].conds.Count; i++)
        {
            if (nodes[nodeId].conds[i] != null)
            {
                nodes[nodeId].conds[i].condition = newCondition;
            }
            else
                Debug.Log("Update condition - doesnt exist");
        }

    }
    /// <summary>
    /// Update action list of target node
    /// </summary>
    /// <param name="nodeId">target node ID</param>
    /// <param name="actions">Monobeh. actions list</param>
    public void UpdateActions(int nodeId, Action[] actions)
    {
        //nodes[nodeId].actions = actions;
        List<LevelDAGNodeAction> lActions = new List<LevelDAGNodeAction>();
        foreach (Action act in actions)
        {
            Debug.Log("act: " + act);
            string name = act.gameObject.name;
            string id = act.id;
            lActions.Add(new LevelDAGNodeAction(name, id));
        }
        nodes[nodeId].lActions = lActions;
        Debug.Log("levelDAG: " + nodeId + " actions updated");
    }

    /// <summary>
    /// Get null node, or after serialization node with id == 0
    /// </summary>
    /// <returns>Smallest free node-ID</returns>
    public int getFreeId()
    {
        for (int i = 1; i < nodes.Length; i++)
        {
            if (nodes[i] == null || nodes[i].id == 0)
                return i;
        }
        return -1;
    }



    /*DEBUG*/
    public void Log()
    {
        Debug.Log("----- LevelDAG ----- ");
        Debug.Log("free ID " + getFreeId().ToString());
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                Debug.Log(i + ". children: ");
                /*
                foreach(KeyValuePair<int, LevelDAGNodeCondition> cond in nodes[i].conds)
                {
                    Debug.Log(i + " cond: " + cond.Value.id + " -> " + cond.Value.child);
                }
                */
                // if (nodes[i].conds[0].condition != null)
                //for (int j = 0; j < nodes[i].conds.Count - 1; j++)
                //{
                //    //Debug.Log(i + " cond: " + nodes[i].conds[j].id + " -> " + nodes[i].conds[j].child);
                //    Debug.Log(i + " cond: " + nodes[i].conds[j] + " -> " + nodes[i].conds[j]);
                //}
                foreach (LevelDAGNodeCondition cond in nodes[i].conds)
                {
                    Debug.Log(i + " cond: " + cond.id + " -> " + cond.child);
                }
                Debug.Log(i + ". END children: ");
                //Log ACTIONS
                if (nodes[i].lActions != null)
                {
                    Debug.Log("Logging actions");
                    foreach (LevelDAGNodeAction act in nodes[i].lActions)
                    {
                        Debug.Log(i + "action: " + act.name + ", " + act.id);
                    }
                }
            }
        }
    }


    /*SERIALIZATION STUFF*/
    public void Save()
    {
        /*
        string json = "{";
        for(int i = 0; i < nodes.Length; i++)
        {
            if(nodes[i] != null)
            {
                json += i + " : {"; //node
                json += "conditions : [";   //condition array start
                foreach(KeyValuePair<int, LevelDAGNodeCondition> cond in nodes[i].conds)
                {
                    json += JsonUtility.ToJson(cond.Value) + ", ";  //comma not at the end
                }
                json += "], "; //cond arr end

                if(nodes[i].actions != null)
                {
                    json += "actions : ["; //actions array start
                    foreach (Action act in nodes[i].actions)
                    {
                        json += JsonUtility.ToJson(new LevelDAGNodeAction(act.id, act.name));
                    }
                    json += "] ";
                }
                json += "}, \n"; //node end
            }
        }
        json += "}";
        */
        string json = JsonUtility.ToJson(this);
        string path = "assets/scenes/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "/levelDAG.json";
        File.WriteAllText(path, json);
        Debug.Log(json);
        //SaveSO();
    }

    public void SaveSO()
    {
        Debug.Log("Saving SO");

        LevelDAG asset = this;

        AssetDatabase.CreateAsset(asset, "assets/scenes/" + /*UnityEngine.SceneManagement.SceneManager.GetActiveScene().name*/ "SampleScene" + "/levelDAG.asset");
        /*
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
        */
        //ScriptableObjectUtility.CreateAsset<LevelDAGNode>();
    }

    public void Load()
    {
        string path = "assets/scenes/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "/levelDAG.json";
        string text = File.ReadAllText(path);
        //LevelDAG level = JsonUtility.FromJson<LevelDAG>(text);
        JsonUtility.FromJsonOverwrite(text, this);
        Debug.Log("Data loaded");
        Log();
        //return level;
    }
    //Load for play mode
    public static LevelDAG LoadFromJson(LevelDAG level)
    {
        string path = "assets/scenes/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "/levelDAG.json";
        string text = File.ReadAllText(path);

        //LevelDAG level = ScriptableObject.CreateInstance<LevelDAG>();

        JsonUtility.FromJsonOverwrite(text, level);
        Debug.Log("Level loaded");

        return level;
    }
}







