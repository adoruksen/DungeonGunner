using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region Header BASIC LEVEL DETAILS
    [Space(10)]
    [Header("BASIC LEVEL DETAILS")]
    #endregion Header BASIC LEVEL DETAILS
    #region Tooltip
    [Tooltip("The name for the level")]
    #endregion Tooltip
    public string levelName;


    #region Header ROOM TEMPLATES FOR LEVEL
    [Space(10)]
    [Header("ROOM TEMPLATES FOR LEVEL")]
    #endregion Header ROOM TEMPLATES FOR LEVEL
    #region Tooltip
    [Tooltip("Populate the list with the room templates that you want to be part of the level. You need to ensure that room templates are included for all room node types that are specified in the Room Node Graphs for level.")]
    #endregion Tooltip
    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPHS FOR LEVEL
    [Space(10)]
    [Header("ROOM NODE GRAPHS FOR LEVEL")]
    #endregion Header ROOM NODE GRAPHS FOR LEVEL
    #region Tooltip
    [Tooltip("Populate this list with the room node graphs which should be randomly selected from for the level")]
    #endregion Tooltip
    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation
#if UNITY_EDITOR
    
    //validate scriptable object details entered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;

        //check to make sure that room templates are specified for all the node types in the specified node graphs
        var isEWCorridor = false;
        var isNSCorridor = false;
        var isEntrance = false;

        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null) return;
            if (roomTemplateSO.roomNodeType.isCorridorEW) isEWCorridor = true;
            if (roomTemplateSO.roomNodeType.isCorridorNS) isNSCorridor = true;
            if (roomTemplateSO.roomNodeType.isEntrance) isEntrance = true;
        }

        if(!isEWCorridor) Debug.Log($"In {this.name} : No E/W Corridor Room Type Specified");
        if (!isNSCorridor) Debug.Log($"In {this.name} : No N/S Corridor Room Type Specified");
        if (!isEntrance) Debug.Log($"In {this.name} : No Entrance Room Type Specified");

        //loop through all node graphs
        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null) return;

            //loop through all nodes in node graph
            foreach (RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if (roomNodeSO == null) return;

                //Check that a room template has been specified for each roomNode type
                //Corridors and Entrance already checked
                if (roomNodeSO.roomNodeType.isEntrance || roomNodeSO.roomNodeType.isCorridorEW || roomNodeSO.roomNodeType.isCorridorNS ||
                    roomNodeSO.roomNodeType.isCorridor || roomNodeSO.roomNodeType.isNone)
                    continue;

                var isRoomNodeTypeFound = false;

                //Loop through all room templates to check that this node type has been specified
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null) continue;

                    if(roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                if (!isRoomNodeTypeFound) Debug.Log($"In {this.name}:No room template {roomNodeSO.roomNodeType.name} found for node graph {roomNodeGraph.name}");
            }
        }
    }
#endif
    #endregion Validation
}
