using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public string id; //gonna use the id in the room class to keep track of parents and child rooms
    public string templateID; //room is derived from the room templates. some of the datas stored in room template. gonna use this to access the template if we need to
    public GameObject prefab; //prefab relating to tilemap
    public RoomNodeTypeSO roomNodeType;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds; //grid coordinates 
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositionsArray;
    public List<string> childRoomIDList;
    public string parentRoomID;
    public List<Doorway> doorWayList; //hold this for which doorway connected and which doorway not
    public bool isPositioned = false;
    public InstantiatedRoom instantiatedRoom;
    public bool isLit = false;
    public bool isClearedOfEnemies = false;
    public bool isPreviouslyVisited = false;

    public Room()
    {
        childRoomIDList = new List<string>();
        doorWayList = new List<Doorway>();
    }
}
