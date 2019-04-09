using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    // for different types of movements set these
    public bool vertical = false;
    public float speed = .01f;
    public float offset = 4f;

    private bool positive = true;
    private float startingPos;
    private Vector3 pos
    {
        get
        {
            return this.transform.localPosition;
        }
        set
        {
            this.transform.localPosition = value;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (vertical)
        {
            startingPos = pos.y;
        }
        else
        {
            startingPos = pos.x;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (vertical)
        {
            if (positive)
            {
                if (pos.y >= startingPos + offset) {
                    positive = false;
                }
                pos = new Vector3(pos.x,pos.y+speed,pos.z);
            }
            else
            {
                if (pos.y <= startingPos - offset)
                {
                    positive = true;
                }
                pos = new Vector3(pos.x, pos.y - speed, pos.z);
            }
        }
        else
        {
            if (positive)
            {
                if (pos.x >= startingPos + offset)
                {
                    positive = false;
                }
                pos = new Vector3(pos.x+speed, pos.y, pos.z);
            }
            else
            {
                if (pos.x <= startingPos - offset)
                {
                    positive = true;
                }
                pos = new Vector3(pos.x-speed, pos.y , pos.z);
            }
        }
    }
}
