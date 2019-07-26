using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Runner : MonoBehaviour
{
    public int num_chickens;
    public float time;

    public GameObject FrontPanel;

    float panelTargetAlpha;

    public bool Pause = false;
    public float PanelShowDuration=5;

    public Vector3 minBounds;
    public Vector3 maxBounds;


    void DestroyChickens()
    {
        for (int i=0; i<this.transform.childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
    }

    void FadeFrontPanel()
    {
        Color c = FrontPanel.GetComponent<MeshRenderer>().material.color;
        if (c.a != panelTargetAlpha)
        {
            c.a = Mathf.Lerp(c.a, panelTargetAlpha, Time.deltaTime);
        }
        FrontPanel.GetComponent<MeshRenderer>().material.color = c;
    }

  

    void CreateChickens()
    {
        GameObject g = GameObject.Find("rudy"); //the prefab for the 'rudy' chicken
        for (int i = 0; i < num_chickens; i++)
        {
            GameObject c = GameObject.Instantiate(g); //instatiates rudy
            c.name = "Chicken_" + i; //names the chicken object based on its number
            c.transform.parent = this.transform;

            /*
             * places all of the chickens in random start locations in the game area, ensures they
             * won't spawn in a location outside of view
             */
            float y_val = Random.Range(10.0f, 14.0f);
            float z_val;
            float x_val;

            if (i >= (num_chickens / 2))
            {
                z_val = -3.0f;
                x_val = Random.Range(-8.0f, 2.5f);
            }
            else
            {
                z_val = -3.0f;
                x_val = Random.Range(-8.0f, 5.0f);
            }

            c.GetComponent<Rigidbody>().transform.position = new Vector3(x_val, y_val, z_val);
        }

    }

    private void Reset()
    {
        Pause = false;
        panelTargetAlpha = 0;
        DestroyChickens();
        CreateChickens();
    }

    void Start()
    {
        CreateChickens();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            Reset();
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            panelTargetAlpha=1f;
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            panelTargetAlpha = 0f;
        }

        FadeFrontPanel();

         if ((Get_Num_Chickens() == 0 && !Pause)  ||(Input.GetKeyUp(KeyCode.T)))
        {
            Debug.Log("Winner Winner Chicken Dinner");
            Pause = true;
            panelTargetAlpha = 1f;
            Invoke("Reset", PanelShowDuration);
        }

    }
        /*
         * Decrements the count of the number of chickens 
         */
        /*public void Decrement_Num_Chickens()
        {
            num_chickens--;
        }
        */
        /*
         * Returns the current count of the number of chickens
         * 
         * @return current chicken count
         */
    public int Get_Num_Chickens()
    {
        //return num_chickens;
       // Debug.Log(this.transform.childCount);

       return this.transform.childCount;
    }

    public bool isInBounds(Vector3 p)
    {
        if ((p.x >= minBounds.x) && (p.y >= minBounds.y) && (p.z >= minBounds.z) &&
            (p.x <= maxBounds.x) && (p.y <= maxBounds.y) && (p.z <= maxBounds.z))
            return true;
        else
            return false;
    }
}


