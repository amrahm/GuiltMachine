using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalRoom : MonoBehaviour
{
    Room room;
    Vector2 position;

    public FinalRoom(Room room, Vector2 position)
    {
        this.room = room;
        this.position = position;
    }

    public Room getRoom()
    {
        return room;
    }

    public Vector2 getPosition()
    {
        return position;
    }
}