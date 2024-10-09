using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tile tile; 
    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapHeight = 10;
    [SerializeField] private int seed;
    
    private System.Random _random;
    private Tilemap _tilemap;



    private Room _startingRoom; 

    void Start()
    {
        _startingRoom = new Room(mapWidth, mapHeight, new Vector2Int(0, 0));
    }

    private void GenerateMap()
    {
        _tilemap.ClearAllTiles(); 
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                _tilemap.SetTile(tilePosition, tile); 
            }
        }
    }


    [ContextMenu("BSP")]
    private void Bsp()
    {
        _random  = new System.Random(seed);
        List<Room> rooms = Split(_startingRoom);
        foreach (var room in rooms)
        {
            Debug.Log($"Room Width: {room.Width}, Height: {room.Height}, Position: {room.Position}");
        }
    }

    private List<Room> Split(Room room)
    {
        List<Room> splitRooms = new List<Room>();
        bool divideBotTop = _random.Next(0, 2) == 0;
        float randomCutPercent = (float)_random.Next(1, 10) / 10;

        if (divideBotTop)
        {
            Debug.Log("Divide Bot Top");
            int cutValue = Mathf.FloorToInt(room.Height * randomCutPercent);
            Room botSplitRoom = new Room(room.Width, cutValue, room.Position);
            Room topSplitRoom = new Room(room.Width, room.Height - cutValue, new Vector2Int(room.Position.x, room.Position.y + cutValue));
            splitRooms.Add(botSplitRoom);
            splitRooms.Add(topSplitRoom);
        }
        else
        {
            Debug.Log("Divide Left Right");
            int cutValue = Mathf.FloorToInt(room.Width * randomCutPercent);
            Room leftSplitRoom = new Room(cutValue, room.Height, room.Position);
            Room rightSplitRoom = new Room(room.Width - cutValue, room.Height, new Vector2Int(room.Position.x + cutValue, room.Position.y));
            splitRooms.Add(leftSplitRoom);
            splitRooms.Add(rightSplitRoom);
        }
        return splitRooms;
    }


}

public class Room
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2Int Position { get; set; }
    
    public Room(int width, int height, Vector2Int position)
    {
        Width = width;
        Height = height;
        Position = position;
    }
}
