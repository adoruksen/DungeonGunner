using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D _boxCollider2D;

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();

        //save room collider bounds
        roomColliderBounds = _boxCollider2D.bounds;
    }

    /// <summary>
    /// initialise the instantiated room
    /// </summary>
    /// <param name="roomGameobject"></param>
    public void Initialise(GameObject roomGameobject)
    {
        PopulateTilemapMemberVariables(roomGameobject);

        BlockOffUnusedDoorWays();

        DisableCollisionTilemapRenderer();
    }

    /// <summary>
    /// populate the tilemap and grid member variables
    /// </summary>
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        //get grid component
        grid = roomGameobject.GetComponentInChildren<Grid>();

        //get tilepamps in children
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.CompareTag("groundTilemap")) groundTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag("decoration1Tilemap")) decoration1Tilemap = tilemap;
            else if (tilemap.gameObject.CompareTag("decoration2Tilemap")) decoration2Tilemap = tilemap;
            else if (tilemap.gameObject.CompareTag("frontTilemap")) frontTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag("collisionTilemap")) collisionTilemap = tilemap;
            else if (tilemap.gameObject.CompareTag("minimapTilemap")) minimapTilemap = tilemap;
        }
    }

    /// <summary>
    /// block off unused doorways in the room
    /// </summary>
    private void BlockOffUnusedDoorWays()
    {
        //loop through all doorways
        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected) continue;

            //block unconnected doorways using tiles on tilemaps
            if (collisionTilemap != null) BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            if (minimapTilemap != null) BlockADoorwayOnTilemapLayer(minimapTilemap, doorway);
            if (groundTilemap != null) BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            if (decoration1Tilemap != null) BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            if (decoration2Tilemap != null) BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            if (frontTilemap != null) BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
        }
    }

    /// <summary>
    /// block a doorway on a tilemap layer
    /// </summary>
    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;
            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;
            case Orientation.none:
                break;
        }
    }

    /// <summary>
    /// block doorway horizontally for north and south doorways
    /// </summary>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        //loop through all tiles to copy
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                //get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                //set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// block doorway vertically for east and west doorways
    /// </summary>
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        //loop throug all tiles to copy
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                //get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                //set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }


    /// <summary>
    /// disable collision tilemap renderer
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }
}
