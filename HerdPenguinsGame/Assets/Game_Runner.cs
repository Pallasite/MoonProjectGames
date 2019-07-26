using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Runner : MonoBehaviour
{
    public int num_penguins;
    void Start()
    {
        GameObject g = GameObject.Find("PenguinFBX"); //the prefab for the penguin
        for (int i = 0; i < num_penguins; i++)
        {
            GameObject p = GameObject.Instantiate(g); //instantiates penguin
            p.name = "Penguin_" + i; //names the penguin object based on its number

            /* places all of the penguins in random start locations in the game area, ensures they
             * won't spawn in a location outside of view
             */
            float x_val = Random.Range(-12.0f, 1.0f);
            float y_val = Random.Range(1.0f, 2.0f);
            float z_val;

            int rand_track = Random.Range(1, 3);

            if (rand_track == 3) {
                z_val = -3.0f;
            }
            else if(rand_track == 2) {
                z_val = -5.0f;
            }
            else {
                z_val = -7.0f;
            }

            p.GetComponent<Rigidbody>().transform.position = new Vector3(x_val, y_val, z_val);
        }
    }

    /*
     * Decrements the count of the number of penguins currently outside of the snow
     * enclosure
     */ 
    public void Decrement_Num_Penguins()
    {
        num_penguins--;
    }

    /*
     * Returns the current count of the number of penguins
     * 
     * @return current penguin count
     */ 
    public int Get_Num_Penguins()
    {
        return num_penguins;
    }
}
