using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    //Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;


    [MenuItem("Room Node Graph Editor",menuItem ="Window/Dungeon Editor/Room Node Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        //Define node layout style
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //Load Room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double clicked in the inspector
    /// </summary>
    [OnOpenAsset(0)] //OnOpenAseetAttribute for opening an asset in Unity, Add this attribute to a static method will make the method be called when Umity is about to open an asset.
    //We need the namespace UnityEditor.Callbacks for OnOpenAsset attribute
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph!=null)
        {
            OpenWindow();
            currentRoomNodeGraph = roomNodeGraph;
            return true;
        }
        return false;
    }

   
    private void OnGUI()
    {
        //If a SO of type RoomNodeGraphSO has been selected then process
        if (currentRoomNodeGraph != null)
        {
            //Process Events
            ProcessEvents(Event.current);

            //Draw Room Nodes
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }
    
    private void ProcessEvents(Event currentEvent)
    {
        //get room node that mouse is over if it's null or not currently being draged
        if (currentRoomNode ==null||currentRoomNode.isLeftClickDragging ==false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        //if mouse isn't over a room node
        if (currentRoomNode==null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        //else process room node events
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    /// check to see to mouse is over a room node - if so then return the room node else return nul
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count-1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //Process Mouse Down Events
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Process mouse down events on the room node graph ( not over a node)
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // Process right click mouse down on graph event (show context menu
        if (currentEvent.button ==1)
        {
            //currentEvent.button ==1 => right click
            ShowContextMenu(currentEvent.mousePosition);
        }
    }

    /// <summary>
    /// Show context menu
    /// </summary>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        //GenericMenu class lets us create custom context menus and dropdown menus.

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }
    /// <summary>
    /// Create a room node at the mousePosition
    /// </summary>
    private void CreateRoomNode(object mousePositionObject)
    {
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone)); //thing i want to find is x.isNone
    }

    /// <summary>
    /// Create a room node at the mouse position - overloaded to also pass in RoomNodeType
    /// </summary>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        //overloaded version of CreateRoomNode function.

        //when first room node created, i am casting mousePositionObject to vec2.
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //create room node scriptable object asset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //add room node to current room node graph room node list
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //set room node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Draw room nodes in the graph window
    /// </summary>
    private void DrawRoomNodes()
    {
        //Loop through all room nodes and draw them
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNodeStyle);
        }

        GUI.changed = true;
    }
}
