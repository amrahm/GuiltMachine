using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Create : MonoBehaviour
{
    static List<Block> area = new List<Block>();
    static List<FinalRoom> finalArea = new List<FinalRoom>();
    static GameObject container;
    static float maxJump = 2f;
    static float blockWidth = 2.5f;
    static float blockHeight = 1f;

    // for instantatite
    public GameObject GO,GO1,GO2;
    public GameObject b1, b2, b3, b4, b5, b6, b7, b8;
    public GameObject g1, g2, g3, g4, g5, g6,g7;
    private Block group1, group2, group3, group4, group5, group6, group7;

    private static float offsetFloat()
    {
        float determineOffset = Random.Range(0f, 1f);
        float offset;
        if (determineOffset < .5f)
        {
            offset = Random.Range(-blockWidth * 2, -blockWidth);
        }
        else
        {
            offset = Random.Range(blockWidth, blockWidth * 2);
        }
        return offset;
    }

    private static bool conflicts(Block add)
    {
        foreach (Block b in area)
        {
            if(add.getHeight() < (b.getHeight() + 2*blockHeight) && add.getHeight() > (b.getHeight() - 2*blockHeight))
            {
                if (add.getPosition().x > b.getPosition().y || add.getPosition().y < b.getPosition().x)
                {
                    continue;
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static Block GetBlock(Block parent,float offsetX, float offsetY)
    {
        float determineBlock = Random.Range(0f, 1f);
        if (determineBlock < .9f)
        {
            // get pos x or random between x and y, we'll see which one is better
            return new Block(new Vector2(parent.getPosition().x + offsetX, parent.getPosition().x + blockWidth + offsetX), parent.getHeight() + offsetY);
        }
        else //multi-length platforms
        {
            return new Block(new Vector2(parent.getPosition().x + offsetX, parent.getPosition().x + blockWidth * Random.Range(2,4) + offsetX), parent.getHeight() + offsetY);
        }
    }

    /*
     * populates the area given by lb and rt, for exploration and creature fighting (EXP?)
     * BFS? has to start off with parent = new Block()
     */
    public static List<Block> createArea(Block parent, Vector2 lb, Vector2 rt, List<Block> centralPieces = null,int size = 40)
    {
        if (area.Count > size)
        {
            return area;
        }
        if (centralPieces != null)
        {
            foreach(Block b in centralPieces)
            {
                area.Add(b);
            }
        }
        Block add1, add2, add3, add4;
        //// base case: block << lb OR block >> rt
        //// what can we add: regular one width block, 1+ width block,
        //// building left and right above it if not already exist (check)
        float offsetX1 = Random.Range(-blockWidth * 3, -blockWidth*1.5f);
        float offsetX2 = Random.Range(blockWidth * 1.5f, blockWidth * 3);
        float offsetX3 = Random.Range(-blockWidth * 3, -blockWidth*1.5f);
        float offsetX4 = Random.Range(blockWidth*1.5f, blockWidth * 3);

        float offsetY1 = Random.Range(maxJump - (blockHeight * .5f), maxJump);
        float offsetY2 = Random.Range(maxJump - (blockHeight * .5f), maxJump);
        float offsetY3 = Random.Range(-maxJump, -maxJump + blockHeight * .5f);
        float offsetY4 = Random.Range(-maxJump, -maxJump + blockHeight * .5f);

        add1 = GetBlock(parent, offsetX1, offsetY1);
        add2 = GetBlock(parent, offsetX2, offsetY2);
        add3 = GetBlock(parent, offsetX3, offsetY3);
        add4 = GetBlock(parent, offsetX4, offsetY4);

        if (!conflicts(add1) && (add1.getPosition().x > lb.x && add1.getPosition().y < rt.x) && (add1.getHeight() <= rt.y && add1.getHeight() >= lb.y))
        {
            area.Add(add1);
            createArea(add1, lb, rt);
        }
        if (!conflicts(add2) && (add2.getPosition().x > lb.x && add2.getPosition().y < rt.x) && (add2.getHeight() <= rt.y && add2.getHeight() >= lb.y))
        {
            area.Add(add2);
            createArea(add2, lb, rt);
        }
        if (!conflicts(add3) && (add3.getPosition().x > lb.x && add3.getPosition().y < rt.x) && (add3.getHeight() <= rt.y && add3.getHeight() >= lb.y))
        {
            area.Add(add3);
            createArea(add3, lb, rt);
        }
        if (!conflicts(add4) && (add4.getPosition().x > lb.x && add4.getPosition().y < rt.x) && (add4.getHeight() <= rt.y && add4.getHeight() >= lb.y))
        {
            area.Add(add4);
            createArea(add4, lb, rt);
        }

        return area;
    }



    /*
     * Given our list of blocks, level, instantiate it in our playing field, depicted by lb and rt
     */
    public void Instantiate(List<Block> level)
    {
        List<GameObject> blocks = new List<GameObject>() { b1, b2, b3, b4, b5, b6, b7, b8};
        foreach (Block b in level)
        {
            int determineBlock = Random.Range(0, blocks.Count);
            if (b.getPosition().y - b.getPosition().x > blockWidth)
            {
                print("multi");
                for(float i = 0; i <= b.getPosition().y - b.getPosition().x; i += blockWidth) {
                    GameObject temp = Instantiate(blocks[determineBlock]);
                    temp.transform.localPosition = new Vector3(b.getPosition().x+i, b.getHeight());
                    temp.transform.SetParent(container.transform);
                }

            }
            else
            {
                GameObject temp = Instantiate(blocks[determineBlock]);
                temp.transform.localPosition = new Vector3(b.getPosition().x, b.getHeight());
                temp.transform.SetParent(container.transform);
            }


        }
    }

    public void NewInstantiate(List<FinalRoom> level)
    {

        foreach (FinalRoom b in level)
        {
            GameObject temp = Instantiate(b.getRoom().getGO());
            temp.transform.localPosition = new Vector3(b.getPosition().x, b.getPosition().y);
            temp.transform.SetParent(container.transform);
        }
    }

    private static bool newConflicts(FinalRoom add, Vector2 lb, Vector2 rt)
    {
        Vector2 addPosition = add.getPosition();
        Vector2 addDimensions = add.getRoom().getDimensions();
        if (addPosition.x > rt.x || addPosition.x < lb.x || addPosition.y > rt.y || addPosition.y < lb.y)
        {
            return true;
        }

        foreach (FinalRoom b in finalArea)
        {
            Vector2 bPosition = b.getPosition();
            Vector2 bDimensions = b.getRoom().getDimensions();
            if (addPosition.y + addDimensions.y/2 > bPosition.y- bDimensions.y/2 &&
             addPosition.y - addDimensions.y / 2 < bPosition.y + bDimensions.y / 2)
            {
                if (addPosition.x + addDimensions.x / 2 > bPosition.x - bDimensions.x / 2 && 
                addPosition.x - addDimensions.x / 2 < bPosition.x + bDimensions.x / 2) 
                    { return true; }

            }
            //if (addPosition.x + addDimensions.x / 2 > bPosition.x - bDimensions.x / 2 ||
            //addPosition.x - addDimensions.x / 2 < bPosition.x + bDimensions.x / 2)
            //{
            //    if (addPosition.y + addDimensions.y / 2 > bPosition.y - bDimensions.y / 2 &&
            // addPosition.y - addDimensions.y / 2 < bPosition.y + bDimensions.y / 2)
            //         { return true; }
            //}
        }
        return false;
    }

    public static List<FinalRoom> newCreateArea(FinalRoom parent, Vector2 lb, Vector2 rt, List<FinalRoom> centralPieces = null, int size = 2)
    {
        finalArea.Add(parent);
        if (finalArea.Count > size)
        {
            return finalArea;
        }
        if (centralPieces != null)
        {
            foreach (FinalRoom b in centralPieces)
            {
                finalArea.Add(b);
            }
        }

        Vector2 parentPosition = parent.getPosition();
        Vector2 parentDimensions = parent.getRoom().getDimensions();
        List<Room> rightPossibleRooms = parent.getRoom().getRooms(Room.Direction.RIGHT);
        int randomIndex;
        if (rightPossibleRooms.Count > 0)
        {
            randomIndex = Random.Range(0, rightPossibleRooms.Count);
            print(randomIndex);
            Room rightChild = rightPossibleRooms[randomIndex];
            Vector2 childDimensions = rightChild.getDimensions();
            print(parentPosition);
            print(parentDimensions);
            print(childDimensions);
            print(parentPosition.x + parentDimensions.x / 2 + childDimensions.x / 2);
            FinalRoom rightRoom = new FinalRoom(rightChild, 
                new Vector2(parentPosition.x + parentDimensions.x/2 + childDimensions.x/2, 
                parentPosition.y-parentDimensions.y/2+childDimensions.y/2));
            if (!newConflicts(rightRoom, lb, rt))
            {
                newCreateArea(rightRoom, lb, rt);
            }

        }

        List<Room> upPossibleRooms = parent.getRoom().getRooms(Room.Direction.UP);
        if (upPossibleRooms.Count > 0) { 
            randomIndex = Random.Range(0, upPossibleRooms.Count);
            print(randomIndex);
            Room upChild = upPossibleRooms[randomIndex];
            Vector2 childDimensions = upChild.getDimensions();
            print(parentPosition);
            print(parentDimensions);
            print(childDimensions);
            print(parentPosition.y + parentDimensions.y / 2 + childDimensions.y / 2);
            FinalRoom upRoom = new FinalRoom(upChild, 
                new Vector2(parentPosition.x - parentDimensions.x / 2 + childDimensions.x / 2,
                parentPosition.y + parentDimensions.y / 2 + childDimensions.y / 2));
            if (!newConflicts(upRoom, lb, rt))
            {
                newCreateArea(upRoom, lb, rt);
            }
        }

        return finalArea;
    }

    private void Start()
    {
        // 10x5
        Dictionary<Room.Direction, List<Room>> test = new Dictionary<Room.Direction, List<Room>>();
        Room group = new Room(new Vector2(10, 5), test, GO);
        test.Add(Room.Direction.UP, new List<Room>());
        test.Add(Room.Direction.RIGHT, new List<Room>());

        // 5x10
        Dictionary<Room.Direction, List<Room>> test1 = new Dictionary<Room.Direction, List<Room>>();
        Room group11 = new Room(new Vector2(5, 10), test, GO1);
        test1.Add(Room.Direction.UP, new List<Room>());
        test1.Add(Room.Direction.RIGHT, new List<Room>());

        // 5x5
        Dictionary<Room.Direction, List<Room>> test2 = new Dictionary<Room.Direction, List<Room>>();
        Room group22 = new Room(new Vector2(5, 5), test, GO2);
        test2.Add(Room.Direction.UP, new List<Room>());
        test2.Add(Room.Direction.RIGHT, new List<Room>());
        // possible rooms declaration
        test[Room.Direction.UP].Add(group);
        test[Room.Direction.UP].Add(group22);
        test[Room.Direction.RIGHT].Add(group);
        test[Room.Direction.RIGHT].Add(group11);
        test[Room.Direction.UP].Add(group);

        test1[Room.Direction.UP].Add(group11);
        test1[Room.Direction.UP].Add(group);
        test1[Room.Direction.RIGHT].Add(group22);

        container = GameObject.Find("Container");
        FinalRoom startingBlock = new FinalRoom(group,new Vector2(100,2));
        List<FinalRoom> x = Create.newCreateArea(startingBlock, new Vector2(100, 2), new Vector2(150, 20));
        NewInstantiate(x);

        //List<Block> x = Create.createArea(new Block(new Vector2(-270,-270),1), new Vector2(-270, 1), new Vector2(-240, 30));

        //List<Block> y = Create.createPath(new Vector2(50, .5f), new Vector2(75, 30));

        //List<Block> z = Create.createPath(new Vector2(110, .5f), new Vector2(100, 28));

        //Instantiate(x);

    }
}

