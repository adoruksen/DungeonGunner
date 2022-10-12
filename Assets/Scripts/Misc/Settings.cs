using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//static class can't be instantiated but we can access the class just by using class name
public static class Settings
{
    #region ROOM SETTINGS

    //max number of child corridors leading from a room. maximum should be 3 although this is not recomended since
    // it can cause the dungeon building to fail since the rooms are more likely to not fit together
    public const int maxChildCorridors = 3;
    #endregion

    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion
}
