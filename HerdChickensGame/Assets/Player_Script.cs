using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Script : MonoBehaviour
{
    public Rigidbody player;
    public Vector3 player_position;

    void Start()
    {
        player = GetComponent<Rigidbody>();
        player.useGravity = false;
        player.isKinematic = true;
    }

    /*
     * called every frame
     */ 
    private void Update()
    {
        /*
         * all of this code controls the movement of the player via the keyboard,
         * eventually this will become irrelevant because the player will be controlled via
         * the kinect user
         */
        Vector3 position = this.transform.position;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            position.x -= 0.25f;
            this.transform.position = position;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            position.x += 0.25f;
            this.transform.position = position;
        }

        player_position = position;
    }
}