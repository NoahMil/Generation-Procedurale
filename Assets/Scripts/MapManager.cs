using System;using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Color = UnityEngine.Color;

public enum StartBy
{
    Horizontal,
    Vertical,
}

public class MapManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tile tile;
    
    [Header("Stats")]
    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapHeight = 10;
    [SerializeField] private int seed;
    [SerializeField] private int iteration;
    

    private System.Random _random;
    private Room _startingRoom;
    private List<Room> _allRooms;
    private List<Vector2> _allRoomsCenters;
    private Dictionary<Room, List<Room>> _allRoomsDictionary;

    
    #region BSP
    [ContextMenu("BSP")]
    private void BinarySpacePartitioning()
    {
        ResetRoomSeed();
        
        SplitRecursive(_allRooms, iteration);
        
        GenerateMap();
        
        CreateAllRoomsCenters(); 
        
        LogRoomData();
        
    }
    private void CreateAllRoomsCenters()
    {
        _allRoomsCenters = new List<Vector2>();

        foreach (Room room in _allRooms)
        {
            _allRoomsCenters.Add(room.CenterPosition);
        }
    }
    private void LogRoomData()
    {
        foreach (Room room in _allRooms)
        {
            Debug.Log($"Room Position: {room.Position}, Width: {room.Width}, Height: {room.Height}, Center: {room.CenterPosition}");
        }
    }

    
    private void ResetRoomSeed()
    {
        _allRooms = new List<Room>();
        _allRooms.Clear();
        _startingRoom = new Room(mapWidth, mapHeight, new Vector2Int(0, 0));
        _allRooms.Add(_startingRoom);
        _random = new System.Random(seed);
    }

    private List<Room> Split(List<Room> rooms)
    {
        Room biggestRoom = FindBiggestRoom(rooms);
        bool divideBotTop = _random.Next(0, 2) == 0; 
        float randomCutPercent = (float)_random.Next(2, 9) / 10; 

        if (divideBotTop)
        {
            int cutValue = Mathf.FloorToInt(biggestRoom.Height * randomCutPercent);
            Room botNewRoom = new Room(biggestRoom.Width, cutValue, biggestRoom.Position);
            Room topNewRoom = new Room(biggestRoom.Width, biggestRoom.Height - cutValue,
                new Vector2Int(biggestRoom.Position.x, biggestRoom.Position.y + cutValue));
            rooms.Add(botNewRoom);
            rooms.Add(topNewRoom);
        }
        else
        {
            int cutValue = Mathf.FloorToInt(biggestRoom.Width * randomCutPercent);
            Room leftNewRoom = new Room(cutValue, biggestRoom.Height, biggestRoom.Position);
            Room rightNewRoom = new Room(biggestRoom.Width - cutValue, biggestRoom.Height,
                new Vector2Int(biggestRoom.Position.x + cutValue, biggestRoom.Position.y));
            rooms.Add(leftNewRoom);
            rooms.Add(rightNewRoom);
        }

        rooms.Remove(biggestRoom);

        return rooms;
    }

    private List<Room> SplitRecursive(List<Room> rooms, int depth)
    {
        if (depth == 0) return rooms;
        rooms = Split(rooms); 
        return SplitRecursive(rooms, depth - 1); 
    }
    
    private Room FindBiggestRoom(List<Room> rooms)
    {
        Room biggestRoom = null;
        int biggestRoomSize = 0;

        foreach (Room room in rooms)
        {
            if (room.Size > biggestRoomSize)
            {
                biggestRoomSize = room.Size;
                biggestRoom = room;
            }
        }

        return biggestRoom;
    }
    #endregion
    
    
    private void OnDrawGizmos()
    {
        if (_allRoomsCenters == null) return; 

        Gizmos.color = Color.red; 

        foreach (Vector2 center in _allRoomsCenters)
        {
            Gizmos.DrawSphere(new Vector3(center.x, center.y, 0), 0.2f); 
        }
    }


    private void DrawRoom(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            float r = (float)_random.NextDouble();
            float g = (float)_random.NextDouble();
            float b = (float)_random.NextDouble();
            Color randomColor = new Color(r, g, b);

            room.Color = randomColor;

            for (int x = 0; x < room.Width; x++)
            {
                for (int y = 0; y < room.Height; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(room.Position.x + x, room.Position.y + y, 0);
                    tilemap.SetTile(tilePosition, tile);
                    tilemap.SetTileFlags(tilePosition, TileFlags.None);
                    tilemap.SetColor(tilePosition, room.Color);
                }
            }
        }
    }
    
    private void GenerateMap()
    {
        tilemap.ClearAllTiles();
        DrawRoom(_allRooms);
    }
}

public class Room
{
    public int Width { get; set; }
    public int Height { get; set; }

    public int Size
    {
        get { return Width * Height; }
    }
    
    public Color Color { get; set; } 
    public Vector2Int Position { get; set; }
    public Vector2 CenterPosition
    {
        get
        {
            int centerX = Position.x + Width / 2;
            int centerY = Position.y + Height / 2;
            return new Vector2(centerX, centerY);
        }
        set => throw new NotImplementedException();
    }

    public Room(int width, int height, Vector2Int position)
    {
        Width = width;
        Height = height;
        Position = position;
    }
}
