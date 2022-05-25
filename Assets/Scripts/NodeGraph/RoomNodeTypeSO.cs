using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="RoomNodeType_",menuName ="Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion
    public bool displayInNodeGraphEditor = true;

    #region Header
    [Header("One Type Should be A Corridor")]
    #endregion
    public bool isCorridor;

    #region Header
    [Header("One Type Should Be a CorridorNS")]
    #endregion
    public bool isCorridorNS;

    #region Header
    [Header("One Type Should Be a CorridorEW")]
    #endregion
    public bool isCorridorEW;

    #region Header
    [Header("One Type Should Be An Entrance")]
    #endregion
    public bool isEntrance;

    #region Header
    [Header("One Type Should Be a Boss Room")]
    #endregion
    public bool isBossRoom;

    #region Header
    [Header("One Type Should Be None {Unassigned}")]
    #endregion
    public bool isNone;

    #region Validation
#if UNITY_EDITOR // <= Be sure that OnValidate Function only run in the editor.

    //OnValidate method is a special method of MonoBehaviour. We using this to detech changes in the inspector. As I update a value in a scriptable object
    //this method will get called
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
        //nameof function will return a string of the field name
    }
#endif
    #endregion
}
