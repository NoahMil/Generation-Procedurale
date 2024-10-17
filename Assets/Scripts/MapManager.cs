using System;using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
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
    
    [Header("Variables")]
    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapHeight = 10;
    [SerializeField] private int margin;
    [SerializeField] private int seed;
    [SerializeField] private int iteration;
    
    private System.Random _random;
    private Room _startingRoom;
    private List<Room> _allRooms;
    private List<Vector2> _allRoomsCenters;
    private Dictionary<Room, List<Room>> _allRoomsDictionary;
    private List<Triangle> _triangulation;


    private List<Triangle> BowyerWatson(List<Vector2> pointList)
{
    _triangulation = new List<Triangle>();
    Triangle superTriangle = CreateSuperTriangle();
    _triangulation.Add(superTriangle);

    foreach (Vector2 point in pointList)
    {
        List<Triangle> badTriangles = new List<Triangle>();
        foreach (Triangle triangle in _triangulation)
        {
            if (triangle.IsPointInCircumcircle(point))
            {
                badTriangles.Add(triangle);
            }
        }

        List<Edge> polygonEdges = new List<Edge>();
        foreach (Triangle badTriangle in badTriangles)
        {
            Edge[] edges = {
                new Edge(badTriangle.Point1, badTriangle.Point2),
                new Edge(badTriangle.Point2, badTriangle.Point3),
                new Edge(badTriangle.Point3, badTriangle.Point1)
            };

            foreach (Edge edge in edges)
            {
                if (polygonEdges.Contains(edge) || polygonEdges.Contains(new Edge(edge.Point2, edge.Point1)))
                {
                    polygonEdges.Remove(edge);
                }
                else
                {
                    polygonEdges.Add(edge);
                }
            }
        }

        foreach (Triangle badTriangle in badTriangles)
        {
            _triangulation.Remove(badTriangle);
        }

        foreach (Edge edge in polygonEdges)
        {
            Triangle newTriangle = new Triangle(edge.Point1, edge.Point2, point);
            _triangulation.Add(newTriangle);
        }
    }

    _triangulation.RemoveAll(triangle =>
        triangle.Point1 == superTriangle.Point1 || triangle.Point1 == superTriangle.Point2 || triangle.Point1 == superTriangle.Point3 ||
        triangle.Point2 == superTriangle.Point1 || triangle.Point2 == superTriangle.Point2 || triangle.Point2 == superTriangle.Point3 ||
        triangle.Point3 == superTriangle.Point1 || triangle.Point3 == superTriangle.Point2 || triangle.Point3 == superTriangle.Point3
    );

    return _triangulation;
}


    private void CreateAllRoomsCenters()
    {
        _allRoomsCenters = new List<Vector2>();

        foreach (Room room in _allRooms)
        {
            _allRoomsCenters.Add(room.CenterPosition);
        }
    }
    
    private Triangle CreateSuperTriangle()
    {
        Vector2 pointTop = new Vector2(0.5f * mapWidth, 2 * mapHeight + margin);
        Vector2 pointLeft = new Vector2(-2 * mapWidth, -2 * mapHeight - margin);
        Vector2 pointRight = new Vector2(2 * mapWidth + mapHeight, -2 * mapHeight - margin);
        Triangle superTriangle = new Triangle(pointTop, pointLeft, pointRight);
        return superTriangle;
    }

    [ContextMenu("Generate BowyerWatson")]
    private void GenerateBowyerWatson()
    {
        _triangulation = BowyerWatson(_allRoomsCenters);
    }
    
    #region BSP
    [ContextMenu("BSP")]
    private void BinarySpacePartitioning()
    {
        ResetRoomSeed();
        
        SplitRecursive(_allRooms, iteration);
        
        GenerateMap();
        
        CreateAllRoomsCenters();
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
        if (_allRoomsCenters == null || _triangulation == null) return;

        Gizmos.color = Color.red;
        foreach (Vector2 center in _allRoomsCenters)
        {
            Gizmos.DrawSphere(new Vector3(center.x, center.y, 0), 0.2f);
        }

        foreach (Triangle triangle in _triangulation)
        {
            Gizmos.color = Color.black;
            DrawTriangle(triangle);
            
        }
    }
    
    private void DrawTriangle(Triangle triangle)
    {
        Gizmos.DrawLine(triangle.Point1, triangle.Point2);
        Gizmos.DrawLine(triangle.Point2, triangle.Point3);
        Gizmos.DrawLine(triangle.Point3, triangle.Point1);
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

public class Triangle
{
    public Vector2 Point1 { get; set; }
    public Vector2 Point2 { get; set; }
    public Vector2 Point3 { get; set; }

    private Vector2 Circumcenter { get; set; }
    private float Circumradius { get; set; }

    public Triangle(Vector2 point1, Vector2 point2, Vector2 point3)
    {
        Point1 = point1;
        Point2 = point2;
        Point3 = point3;
        CalculateCircumcircle();
        
    }

    private void CalculateCircumcircle()
    {
        float d = 2 * (Point1.x * (Point2.y - Point3.y) + Point2.x * (Point3.y - Point1.y) + Point3.x * (Point1.y - Point2.y));

        float centerX = ((Point1.x * Point1.x + Point1.y * Point1.y) * (Point2.y - Point3.y) +
                         (Point2.x * Point2.x + Point2.y * Point2.y) * (Point3.y - Point1.y) +
                         (Point3.x * Point3.x + Point3.y * Point3.y) * (Point1.y - Point2.y)) / d;

        float centerY = ((Point1.x * Point1.x + Point1.y * Point1.y) * (Point3.x - Point2.x) +
                         (Point2.x * Point2.x + Point2.y * Point2.y) * (Point1.x - Point3.x) +
                         (Point3.x * Point3.x + Point3.y * Point3.y) * (Point2.x - Point1.x)) / d;

        Circumcenter = new Vector2(centerX, centerY);

        Circumradius = Vector2.Distance(Circumcenter, Point1);
    }
    
    public bool IsPointInCircumcircle(Vector2 point)
    {
        float dx = point.x - Circumcenter.x;
        float dy = point.y - Circumcenter.y;
        float radiusSquared = Circumradius * Circumradius;
        bool isInCircumcircle = (dx * dx + dy * dy) <= radiusSquared;

        return isInCircumcircle;
    }

}

public class Edge
{
    public Vector2 Point1 { get; private set; }
    public Vector2 Point2 { get; private set; }
    public float Length { get; private set; }

    public Edge(Vector2 point1, Vector2 point2)
    {
        Point1 = point1;
        Point2 = point2;
        Length = Vector2.Distance(point1, point2);
    }

    public override bool Equals(object obj)
    {
        if (obj is Edge otherEdge) // Vérifie si l'object vérifié est un autre edge
        {
            return (Point1 == otherEdge.Point1 && Point2 == otherEdge.Point2) || // Compare les points de l'edge actuel et de l'autre
                   (Point1 == otherEdge.Point2 && Point2 == otherEdge.Point1);
        }
        return false;
    }

    public override int GetHashCode() // S'assure du bon ordre des points
    {
        int hash1 = Point1.GetHashCode() ^ Point2.GetHashCode();
        int hash2 = Point2.GetHashCode() ^ Point1.GetHashCode();
        return hash1 ^ hash2;
    }
}


