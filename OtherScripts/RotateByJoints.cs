using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateByJoints : MonoBehaviour
{
    public string joint1Name;
    public string joint2Name;

    public string up1Name="SpineMid";
    public string up2Name ="SpineBase";

   // public Quaternion orig;
    private GameObject j1;
    private GameObject j2;
    private GameObject u1;
    private GameObject u2;

    

    public float RotOffsetX, RotOffsetY, RotOffsetZ;
    public bool useLeftMost;


    // Start is called before the first frame update
    void Start()
    {
        /*
        j1 = GameObject.Find(joint1Name);
        j2 = GameObject.Find(joint2Name);
        u1 = GameObject.Find(up1Name);
        u2 = GameObject.Find(up2Name);
        */
    }

    GameObject[] FindGameObjectsWithName(string name)
    {
        int a = GameObject.FindObjectsOfType<GameObject>().Length;
        GameObject[] arr = new GameObject[a];
        int FluentNumber = 0;
        for (int i = 0; i < a; i++)
        {
            if (GameObject.FindObjectsOfType<GameObject>()[i].name == name)
            {
                arr[FluentNumber] = GameObject.FindObjectsOfType<GameObject>()[i];
                FluentNumber++;
            }
        }
        System.Array.Resize(ref arr, FluentNumber);
        return arr;
    }

    GameObject FindJointObject(string jointName)
    {
        GameObject refObj = null;
        foreach (GameObject g in FindGameObjectsWithName(jointName))
        {
            if (!refObj)
            {
                refObj = g;
            }
            else 
            {
                if (useLeftMost)
                {
                    if (g.transform.position.x < refObj.transform.position.x)
                        refObj = g;
                }
                else
                {
                    if (g.transform.position.x >= refObj.transform.position.x)
                        refObj = g;
                }
            }
        }
        return refObj;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        j1 = FindJointObject(joint1Name);
        j2 = FindJointObject(joint2Name);
        u1 = FindJointObject(up1Name);
        u2 = FindJointObject(up2Name);



        if (j1 && j2)
        {
            Vector3 relativePos = j2.transform.position - j1.transform.position;
            //flip the y...not sure why
            //change the z direction to match kinect?
           
             relativePos.x = -relativePos.x;
            //  relativePos.y = -relativePos.y;
            relativePos.z = -relativePos.z;


            Vector3 upPos = u1.transform.position - u2.transform.position;
            Vector3 forwardPos = Vector3.Cross(relativePos.normalized, Vector3.up);
            Quaternion rotation = Quaternion.LookRotation(relativePos, upPos.normalized);

            Quaternion rotOffset = Quaternion.Euler(RotOffsetX, RotOffsetY, RotOffsetZ);


           // rotation.z = -rotation.z;
           // rotation.w = -rotation.w;


            this.transform.localRotation = rotation* rotOffset;
        }
        else
        {
            /*
            j1 = GameObject.Find(joint1Name);
            j2 = GameObject.Find(joint2Name);
            u1 = GameObject.Find(up1Name);
            u2 = GameObject.Find(up2Name);
            */
        }
    }
}
