using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> _roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> _roomTemplateList = null;
    private RoomNodeTypeListSO _roomNodeTypeList;
    private bool _dungeonBuildSuccesful;

    protected override void Awake()
    {
        base.Awake();

        //load the room node type list
        LoadRoomNodeTypeList();

        //Set dimmed material to fully visible
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    /// <summary>
    /// Load the room node type list from gameresources class
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        _roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// generate random dungeon, returns true if dungeon built, false if failed
    /// </summary>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        _roomTemplateList = currentDungeonLevel.roomTemplateList;

        //Load the scriptable object room templates into the dictionary
        LoadRoomTemplatesIntoDictionary();

        _dungeonBuildSuccesful = false;
        var dungeonBuildAttempts = 0;

        while(!_dungeonBuildSuccesful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            //select random room node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            var dungeonRebuildAttemptsForNodeGraph = 0;
            _dungeonBuildSuccesful = false;

            //loop until dungeon succesfully built or more than max attempts for node graph
            while (!_dungeonBuildSuccesful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                //Clear dungeon room gameobjects and dungeon room dictionary
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                //Attempt to build a random dungeon for the selected room node graph
                _dungeonBuildSuccesful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }
            if (_dungeonBuildSuccesful) InstantiateRoomGameobjects(); //instantiate room gameobjects
        }
        return _dungeonBuildSuccesful;
    }

    /// <summary>
    /// load the room templates into the dictionary
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        //Clear room template dictionary
        _roomTemplateDictionary.Clear();

        //Load room template list into dictionary
        foreach (RoomTemplateSO roomTemplate in _roomTemplateList)
        {
            if (!_roomTemplateDictionary.ContainsKey(roomTemplate.guid)) _roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            else Debug.Log($"Duplicate room template key in {_roomTemplateList}");
        }
    }

    /// <summary>
    /// Attempt to randomlu build the dungeon for the specified room node graph. returns true if a
    /// succesful random layout was generated, else return false if a problem was encountered and
    /// another attempt is required
    /// </summary>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        //create open room node queue
        var openRoomNodeQueue = new Queue<RoomNodeSO>();

        //add entrance node to room node queue from room node graph
        var entranceNode = roomNodeGraph.GetRoomNode(_roomNodeTypeList.list.Find(x => x.isEntrance)); //find x where x isEntrance

        if (entranceNode != null) openRoomNodeQueue.Enqueue(entranceNode);
        else
        {
            Debug.Log("No Entrance Node");
            return false;
        }

        //Start with no room overlaps
        var noRoomOverlaps = true;

        //process open room nodes queue
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        //if all the room nodes have been processed and there hasn't been a room overlap then return true
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps) return true;
        else return false;
    }

    /// <summary>
    /// process rooms in the open room node queue, returning true if there are no room overlaps
    /// </summary>
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph,Queue<RoomNodeSO> openRoomNodeQueue,bool noRoomOverlaps)
    {
        //while room nodes in open room node queue & no room overlaps detected
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            //Get next room node from open room node queue.
            var roomNode = openRoomNodeQueue.Dequeue(); //getting the first room node from queue

            //add child nodes to queue from room node graph (with links to this parent Room)
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            //if the room is the enetrance mark as positioned and add to room dictionary
            if (roomNode.roomNodeType.isEntrance)
            {
                var roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                var room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                //Add room to room dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }

            //else if the room type isn't an entrance
            else
            {
                //else get parent room for node
                var parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                //see if room can be placed without overlaps
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }

        }

        return noRoomOverlaps;
    }

    /// <summary>
    /// atempt to place the room node in the dungeon if room can be placed return the room, else return null
    /// </summary>
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        //initialise and assume overlap until proven otherwise
        var roomOverlaps = true;

        //Do While room overlaps - try to place against all available doorways of the parent until the room is succesfully placed without overlap.
        while (roomOverlaps)
        {
            //select random unconnected available doorway for parent
            var unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0) return false; //room overlaps

            var doorwayParent = unconnectedAvailableParentDoorways[Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            //get a random orom template for room node that is consistent with the parent door orientation
            var roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            //create room
            var room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            //place the room - returns true if the room doesn't overlap
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                //if room doesn't overlap then set to false to exit while loop
                roomOverlaps = false;

                //mark room as positioned
                room.isPositioned = true;

                //add room to dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else roomOverlaps = true;
        }
        return true; //no rooms overlap
    }

    /// <summary>
    /// get a random room template for room node taking into account the parent doorway orientation
    /// </summary>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        //if room node is a corridor then select random corrent corridor room template based on parent doorway orientation
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(_roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(_roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;
                case Orientation.none:
                    break;
                default:
                    break;
            }
        }

        //else select random room template
        else roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        return roomTemplate;
    }

    /// <summary>
    /// place the room- returns true if the room doesn't overlap, false otherwise
    /// </summary>
    private bool PlaceTheRoom(Room parentRoom,Doorway doorwayParent,Room room)
    {
        //get current room doorway position
        var doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        //return if no doorway in room opposite to parent doorway
        if(doorway == null)
        {
            //just mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;
            return false;
        }

        //calculate "world" grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;
        //calculate adjustment position offset based on room doorway position that we are trying to connect ( e.g. if this doorway is west then we need to add (1,0) to the east parent doorway

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none:
                break;
            default:
                break;
        }

        //calculate room lower bounds and upper bound on positioning to align with parent doorway
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        var overlappingRoom = CheckForRoomOverlap(room);
        if(overlappingRoom == null)
        {
            //mark doorways as connected and unavailable
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;
            doorway.isConnected = true;
            doorway.isUnavailable = true;

            //return true to show rooms have been connected with no overlap
            return true;
        }

        else
        {
            //just mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;
            return false;

        }

    }

    /// <summary>
    /// get the doorway from the doorway list that has the opposite orientation to doorway
    /// bc one door is entrance to the room, other one is the place that i will put the corridor
    /// </summary>
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west) return doorwayToCheck;
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east) return doorwayToCheck;
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south) return doorwayToCheck;
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north) return doorwayToCheck;
        }
        return null;
    }

    /// <summary>
    /// check for rooms that overlap the upper and lower bounds parameters and if there are overlapping rooms then return room else return null
    /// </summary>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        //iterate through all rooms
        foreach (KeyValuePair<string,Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            var room = keyValuePair.Value;

            //skip if same room as room to test or room hasn't been positioned
            if (room.id == roomToTest.id || !room.isPositioned) continue;

            //if room overlaps
            if (IsOverlappingRoom(roomToTest, room)) return room;
        }
        return null;
    }

    /// <summary>
    /// check if 2 rooms overlap each other, return true if they overlap or false f they don't overlap
    /// </summary>
    private bool IsOverlappingRoom(Room room1, Room room2)
    {
        var isOVerlappingX = IsOverlappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        var isOverlappingY = IsOverlappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if (isOVerlappingX && isOverlappingY) return true;
        else return false;
    }

    /// <summary>
    /// check if interval 1 overlaps interval 2 - this method is used by the IsOverlappingRoom method
    /// </summary>
    private bool IsOverlappingInterval(int imin1,int imax1,int imin2,int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2)) return true;
        else return false;
    }

    /// <summary>
    /// get a random room template from the roomtemplatelist that matches the roomtype and return it
    /// return null is no mathing room templates found
    /// </summary>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        var matchingRoomTemplateList = new List<RoomTemplateSO>();
        foreach (RoomTemplateSO roomTemplate in _roomTemplateList)
        {
            //add matching room templates
            if (roomTemplate.roomNodeType == roomNodeType) matchingRoomTemplateList.Add(roomTemplate);
        }

        //return null if list is zero
        if (matchingRoomTemplateList.Count == 0) return null;

        //select random room template from list and return
        return matchingRoomTemplateList[Random.Range(0, matchingRoomTemplateList.Count)];
    }

    /// <summary>
    /// get unconnected doorways
    /// </summary>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        //loop through doorway list
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable) yield return doorway;
        }
    }

    /// <summary>
    /// create room based on roomTemplate and layoutNode and return the created room
    /// </summary>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        //initialise room from template
        var room = new Room();
        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionsArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList); //did this bc i don't want to reference this. just want copy list
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        //set parent id for room
        if (roomNode.parentRoomNodeIDList.Count == 0) //entrance
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else room.parentRoomID = roomNode.parentRoomNodeIDList[0];

        return room;
    }

    /// <summary>
    /// select random room node graph from the list of room node graphs
    /// </summary>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0) return roomNodeGraphList[Random.Range(0, roomNodeGraphList.Count)];
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }

    /// <summary>
    /// Create deep copy of string list
    /// </summary>
    private List<string> CopyStringList(List<string> oldStringList)
    {
        var newStringList = new List<string>();

        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }
        return newStringList;
    }

    /// <summary>
    /// create deep copy of doorway list
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        var newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in oldDoorwayList)
        {
            var newDoorway = new Doorway();
            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }

    /// <summary>
    /// instantiate the dungeon room gameobjects from the prefabs
    /// </summary>
    private void InstantiateRoomGameobjects()
    {

    }


    /// <summary>
    /// get a room template by room template id, returns null if id doesn't exist
    /// </summary>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (_roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate)) return roomTemplate;
        else return null;
    }

    /// <summary>
    /// get room by roomid , if no room exist with that id return null
    /// </summary>
    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room)) return room;
        else return null;
    }

    /// <summary>
    /// clear dungeon room gameobjects and dungeon room dictionary
    /// </summary>
    private void ClearDungeon()
    {
        //Destroy instantiated dungeon gameobjects and clear dungeon manager room dictionary
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string,Room> keyValuePair in dungeonBuilderRoomDictionary)
            {
                var room = keyValuePair.Value;
                if (room.instantiatedRoom != null) Destroy(room.instantiatedRoom.gameObject);
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

}
