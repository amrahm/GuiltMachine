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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

///*
//     * create a path from start to end, e.g. like we have a treasure we want to be accessible from a certain point
//     */
//public static List<Block> createPath(Vector2 start, Vector2 end)
//{
//    if (area.Count > 40)
//    {
//        return area;
//    }
//    if (start.x > end.x - 5 && start.x < end.x + 5 && start.y > end.y - 5 && start.y < end.y + 5)
//    {
//        return area;
//    }
//    Block add1, add2;
//    //// base case: block << lb OR block >> rt
//    //// what can we add: regular one width block, 1+ width block,
//    //// building left and right above it if not already exist (check)
//    float offsetX1 = Random.Range(blockWidth * .5f, blockWidth * 1.5f);
//    float offsetX2 = Random.Range(blockWidth * 1.5f, blockWidth * 3f);

//    float offsetY1 = Random.Range(maxJump - (blockHeight * .5f), maxJump);
//    float offsetY2 = Random.Range(maxJump - (blockHeight * .5f), maxJump);

//    add1 = GetBlock(new Block(new Vector2(start.x, start.x), start.y), offsetX1, offsetY1);
//    add2 = GetBlock(new Block(new Vector2(start.x, start.x), start.y), offsetX2, offsetY2);

//    if (!conflicts(add1) && add1.getHeight() < end.y)
//    {
//        area.Add(add1);
//        createPath(new Vector2(add1.getPosition().x, add1.getHeight()), end);
//    }
//    if (!conflicts(add2) && add2.getHeight() < end.y)
//    {
//        area.Add(add2);
//        createPath(new Vector2(add2.getPosition().x, add2.getHeight()), end);
//    }

//    return area;
//    //// different way that is more clean? less block like creation, more pathlike
//    //return createArea(new Block(new Vector2(start.x,start.x),start.y), start, end);
//}
