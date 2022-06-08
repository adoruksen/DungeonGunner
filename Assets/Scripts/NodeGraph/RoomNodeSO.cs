using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code

    //the following code should only be run in the Unity Editor
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// Initialise node
    /// </summary>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name="Room Node";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        //Load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draw node with the nodeStyle
    /// </summary>
    public void Draw(GUIStyle nodeStyle)
    {
        //Draw Node Box Using Begin Area
        GUILayout.BeginArea(rect, nodeStyle);

        //Start Region To Detect Popup Selection Changes
        EditorGUI.BeginChangeCheck();

        //Display a popup using the RoomNodeType name values that can be selected from (default to the currently set roomNodeType)
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

        roomNodeType = roomNodeTypeList.list[selection];

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Populate a string array with the room node types to display that can be selected
    /// </summary>
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }

    /// <summary>
    /// process events for the node
    /// </summary>
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //left click down
        if (currentEvent.button==0)
        {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button ==1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    /// <summary>
    /// process left click down event
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        //selection.activeObject actually locked in the node object that we are selected from editorWindow
        //that means when we select an object at editorWindow, object is selected at UnityEditor too..
        Selection.activeObject = this;

        //toggle node selection
        if (isSelected==true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
        //isSelected = !isSelected; doing the same job
    }

    /// <summary>
    /// Process right click down ( draw lines)
    /// </summary>
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// process mouse up event
    /// </summary
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// Process left click up event
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// Process mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button==0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// Process Left mouse drag event
    /// </summary>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;

        //currentEvent.delta captures the relative movement of the mouse compared to the last event.
        //so that is going to be how much our mouse is moving by.
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// Drag Node
    /// </summary>
    public void DragNode(Vector2 delta)
    {
        //we move the node position by delta value
        rect.position += delta;

        //setDirty tells unity that somethings's happened on this assets.So we need to save it
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Add childID to the node (returns true if the node has been added , otherwise false
    /// </summary>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        childRoomNodeIDList.Add(childID);
        return true;
    }

    /// <summary>
    /// Add parentID to the node ( returns true if the node has been added , otherwise false
    /// </summary>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

#endif

    #endregion Editor Code
}
