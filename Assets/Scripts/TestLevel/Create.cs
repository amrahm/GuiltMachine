using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Create : MonoBehaviour
{
    static List<Block> area = new List<Block>();
    static List<FinalBlock> finalArea = new List<FinalBlock>();
    static GameObject container;
    static float maxJump = 2f;
    static float blockWidth = 2.5f;
    static float blockHeight = 1f;

    // for instantatite
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

    public void NewInstantiate(List<FinalBlock> level)
    {

        foreach (FinalBlock b in level)
        {
            GameObject temp = Instantiate(b.getBlock().getGO());
            temp.transform.localPosition = new Vector3(b.getPosition().x, b.getPosition().y);
            temp.transform.SetParent(container.transform);
        }
    }

    private static bool newConflicts(FinalBlock add)
    {
        Vector2 addPosition = add.getPosition();
        foreach (FinalBlock b in finalArea)
        {
            Vector2 position = b.getPosition();
            // need a continue and a true
            if(addPosition.y > position.y && addPosition.y < position.y + b.getBlock().getHeight())
            {
                if (addPosition.x > position.x && addPosition.x < position.x + b.getBlock().getWidth())
                {
                    return true;
                }
            } else if (position.y > addPosition.y && position.y < addPosition.y + b.getBlock().getHeight())
            {
                if (position.x > addPosition.x && position.x < addPosition.x + b.getBlock().getWidth())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static List<FinalBlock> newCreateArea(FinalBlock parent, Vector2 lb, Vector2 rt, List<Block> centralPieces = null, int size = 2)
    {
        finalArea.Add(parent);
        print(finalArea.Count);
        if (finalArea.Count > size)
        {
            return finalArea;
        }
        if (centralPieces != null)
        {
            foreach (Block b in centralPieces)
            {
                area.Add(b);
            }
        }
        Block tempBlock = parent.getBlock();
        print(tempBlock.getGO());
        Dictionary<string, List<Block>> tempDict = tempBlock.getPossible();
        foreach (string key in tempDict.Keys)
        {
            Vector2 position = parent.getPosition();
            Block randomBlock = tempDict[key][Random.Range(0, tempDict[key].Count)];
            foreach (Block b in tempDict[key])
            {
                print(b.getGO());
            }
            FinalBlock temp;
            print(key);
            if (key == "top")
            {
                 temp = new FinalBlock(randomBlock, new Vector2(position.x, position.y + tempBlock.getHeight()/2 + randomBlock.getHeight()/2));
            } else if (key == "right-top")
            {
                 temp = new FinalBlock(randomBlock, new Vector2(position.x + tempBlock.getWidth() / 2 + randomBlock.getWidth() / 2, position.y + tempBlock.getHeight() / 2 + randomBlock.getHeight() / 2));

            } else if (key == "right")
            {
                 temp = new FinalBlock(randomBlock, new Vector2(position.x + tempBlock.getWidth() / 2 + randomBlock.getWidth() / 2, position.y));
            } else if (key == "left")
            {
                 temp = new FinalBlock(randomBlock, new Vector2(position.x - tempBlock.getWidth() / 2 - randomBlock.getHeight() / 2, position.y));
            } else // key == "left-top"
            {
                 temp = new FinalBlock(randomBlock, new Vector2(position.x - tempBlock.getWidth() / 2 - randomBlock.getHeight() / 2, position.y + tempBlock.getHeight() / 2 + randomBlock.getHeight() / 2));
            }
            if (!newConflicts(temp)) { newCreateArea(temp, lb, rt); }
        }

        return finalArea;

    }

    private void Start()
    {
        // stairs right short
        group1 = new Block(g1, new Vector2(6, 5));

        //stairs right
        group2 = new Block(g2, new Vector2(11, 5));


        //stairs left short
        group3 = new Block(g3, new Vector2(6, 3));

        //stairs left
        group4 = new Block(g4, new Vector2(11, 5));



        //square
        group5 = new Block(g5, new Vector2(12, 6.5f));

        // moving
        group6 = new Block(g6, new Vector2(15, 7.5f));

        // platform
        group7 = new Block(g7, new Vector2(14, 5));

        group1.addDict(new Dictionary<string, List<Block>>(){ { "top", new List<Block>{group3 } },
         { "right-top" , new List<Block>{group1} } , { "right", new List<Block>{group5}}});
        group2.addDict(new Dictionary<string, List<Block>>(){ { "top", new List<Block>{group3 } },
         { "right-top" , new List<Block>{group1} }, { "right", new List<Block>{group5}} });
        group3.addDict(new Dictionary<string, List<Block>>(){ { "top", new List<Block>{group1 } },
         { "left-top" , new List<Block>{group3} } , { "left", new List<Block>{group5}}});
        group4.addDict(new Dictionary<string, List<Block>>(){ { "top", new List<Block>{group1 } },
         { "left-top" , new List<Block>{group3} } , { "left", new List<Block>{group5}} });
        group5.addDict(new Dictionary<string, List<Block>>() { { "right", new List<Block> { group7, group6, group2 } },
            { "top" ,new List<Block> { group3 } }  });
        group6.addDict(new Dictionary<string, List<Block>>() { { "right-top", new List<Block> { group5, group2 } },
            { "top" ,new List<Block> { group3 } } });
        group7.addDict(new Dictionary<string, List<Block>>() { { "right", new List<Block> { group2, group6 } } });
        print(group3.getGO().name);
        print(group1.getPossible()["top"][0].getGO().name);
        container = GameObject.Find("Container");
        FinalBlock startingBlock = new FinalBlock(group2, new Vector2(100, 2));
        List<FinalBlock> x = Create.newCreateArea(startingBlock, new Vector2(100, 2), new Vector2(150, 20));
        NewInstantiate(x);

        //List<Block> x = Create.createArea(new Block(new Vector2(-270,-270),1), new Vector2(-270, 1), new Vector2(-240, 30));

        //List<Block> y = Create.createPath(new Vector2(50, .5f), new Vector2(75, 30));

        //List<Block> z = Create.createPath(new Vector2(110, .5f), new Vector2(100, 28));

        //Instantiate(x);

    }
}

