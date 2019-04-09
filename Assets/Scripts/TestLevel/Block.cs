using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
Dictionary<string, List<Block>> possibleRightBlocks = new Dictionary<string, List<Block>>();
    Vector2 left_right;
    float height;
    float width;
    bool isMoving = false; // work on later second
    bool isReappearing = false; // work on later third
    //float moveOffset;
    GameObject go;

    public Block()
    {
    }

    public Block(GameObject go, Vector2 dimensions)
    {
        this.go = go;
        this.height = dimensions.y;
        this.width = dimensions.x;
    }

    public Block(Vector2 lr, float height)
    {
       this.height = height;
        left_right = new Vector2(lr.x,lr.y);
        width = lr.y - lr.x;
    }

    //public void setMoveOffset(float move)
    //{
    //    if (isMoving) {
    //        moveOffset = move;
    //        }
    //}
    public void addDict(Dictionary<string, List<Block>> possible)
    {
        this.possibleRightBlocks = possible;
    }
    public Vector2 getPosition()
    {
        return new Vector2(left_right.x, left_right.y);
    }

    public GameObject getGO()
    {
        return go;
    }

    public Dictionary<string, List<Block>> getPossible()
    {
        return possibleRightBlocks;
    }

    public float getHeight()
    {
        return this.height;
    }

    public float getWidth()
    {
        return this.width;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {

        }
    }
}
