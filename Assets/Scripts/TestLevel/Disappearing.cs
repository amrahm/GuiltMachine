using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Attach this to a block to make it a disappearing/repearring block
 */
public class Disappearing : MonoBehaviour
{
    public float speed = .01f; // of disappearing block
    public float offset = 0f; // offset of when the block first starts disappearing/appearing
    private bool appearing = false;
    private float transparency
    {
        get
        {
           return this.gameObject.GetComponent<SpriteRenderer>().color.a;        }
        set
        {
            Color newColor = this.gameObject.GetComponent<SpriteRenderer>().color;
            newColor.a = value;
            this.gameObject.GetComponent<SpriteRenderer>().color = newColor;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time > offset) {
            if (appearing)
            {
                if (transparency >= .5f) { // can only stand on block if transparency is above .5
                    this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                }
                if (transparency >= 1f)
                {
                    appearing = false;
                }
                transparency += speed;
            }
            else
            {
                if (transparency <= .5f)
                {
                    this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }
                if (transparency <= 0f)
                {
                    appearing = true;
                }
                transparency -= speed;
            }
        }
    }
}
