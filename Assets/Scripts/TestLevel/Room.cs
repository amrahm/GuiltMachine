using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum Direction { UP,RIGHT};
    private Dictionary<Direction, List<Room>> roomDictionary = new Dictionary<Direction, List<Room>>();
    private Vector2 dimensions;
    private Vector2 offsetDimensions;
    public GameObject GO;

    public Room(Vector2 dimensions, Dictionary<Direction, List<Room>> roomDictionary, GameObject GO, Vector2 offsetDimensions = new Vector2())
    {
        this.dimensions = dimensions;
        this.roomDictionary = roomDictionary;
        this.GO = GO;
        this.offsetDimensions = offsetDimensions;
    }

    public List<Room> getRooms(Direction direction)
    {
        return roomDictionary[direction];
    }

    public Vector2 getDimensions()
    {
        return dimensions;
    }
    public GameObject getGO()
    {
        return GO;
    }

    public Vector2 getOffset()
    {
        return offsetDimensions;
    }
}
