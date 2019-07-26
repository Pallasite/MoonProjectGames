using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class KinectBodyUDP : MonoBehaviour
{

    // port number
    public int receivePort = 53715;

    private UdpClient receivingUdpClient;
    private IPEndPoint remoteIpEndPoint;
    private Thread thread;

    //public Vector3 bodyOffset;// = new Vector3 (-.98f, -3.44f, -34.3f);
    //private Vector3 kinectOffset = new Vector3 (-3.1f, 0.3109993f, 0);
    //private Vector3 kinectOffset = new Vector3 (-3.1f, -0.1f, 0);

    public bool ShowBody = true;
    public bool EnableCollisions = true;
    public float JointScale = .2f;
    public float JointOffsetScale = 2f;
    public float RemovalTime = 1;



   
    private bool exit = false;

    //public GameObject[] jointObjs;

    //public GameObject[] jointRefs;



    static public int MaxNumPlayers = 6;

    List<GameObject> Players = new List<GameObject>();
    static KinectBody[] KinectBodyContainers = new KinectBody[MaxNumPlayers];



    //from kinect
    enum _JointType
    {
        JointType_SpineBase = 0,
        JointType_SpineMid = 1,
        JointType_Neck = 2,
        JointType_Head = 3,
        JointType_ShoulderLeft = 4,
        JointType_ElbowLeft = 5,
        JointType_WristLeft = 6,
        JointType_HandLeft = 7,
        JointType_ShoulderRight = 8,
        JointType_ElbowRight = 9,
        JointType_WristRight = 10,
        JointType_HandRight = 11,
        JointType_HipLeft = 12,
        JointType_KneeLeft = 13,
        JointType_AnkleLeft = 14,
        JointType_FootLeft = 15,
        JointType_HipRight = 16,
        JointType_KneeRight = 17,
        JointType_AnkleRight = 18,
        JointType_FootRight = 19,
        JointType_SpineShoulder = 20,
        JointType_HandTipLeft = 21,
        JointType_ThumbLeft = 22,
        JointType_HandTipRight = 23,
        JointType_ThumbRight = 24,
        JointType_Count = (JointType_ThumbRight + 1)
    };





    const int NumJoints = 25;
    /* int[] jointPos = new int[NumJoints * 4];
     public static int[] hands = new int[2];
     int[] prevhands = new int[2];
     */

    public class KinectBody : MonoBehaviour
    {
        public UInt64[] id = new UInt64[1];
        public float[] jointPos = new float[NumJoints * 4];
        public float[] jointRots = new float[NumJoints * 4];
        public int[] hands = new int[2];
        public float updateTime;

        //unity stuff
        public GameObject[] jointObjs = new GameObject[NumJoints];

        //setting 
        public bool ShowBody = true;
        public bool EnableCollisions = true;
        public float JointScale = .2f;
        public float JointOffsetScale = 2f;

        public void Copy(KinectBody k)
        {
    
            id[0] = k.id[0];
            Buffer.BlockCopy(k.jointPos, 0, jointPos, 0, sizeof(float) * NumJoints * 4);
            Buffer.BlockCopy(k.jointRots, 0, jointRots, 0, sizeof(float) * NumJoints * 4);
            Buffer.BlockCopy(k.hands, 0, hands, 0, sizeof(int) * 2);

            //set the update time
            updateTime = Time.time;

            //now set the data to be invalid as we have already copied it in
            k.id[0] = 0;

        }

        public void SetupBody(KinectBodyUDP settings)
        {
            //copy the settings
            ShowBody = settings.ShowBody;
            EnableCollisions = settings.EnableCollisions;
            JointScale = settings.JointScale;
            JointOffsetScale = settings.JointOffsetScale;
            //update time
            updateTime = Time.time;


            for (int i = 0; i < NumJoints; i++)
            {
                jointObjs[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jointObjs[i].transform.localScale = new Vector3(JointScale, JointScale, JointScale);
                jointObjs[i].GetComponent<Renderer>().enabled = ShowBody;
                jointObjs[i].GetComponent<Collider>().enabled = EnableCollisions;
                jointObjs[i].transform.parent = this.transform;
                jointObjs[i].tag = "Player";
                //SphereCollider sc = jointObjs[i].AddComponent<SphereCollider>();
                //Rigidbody rb = jointObjs[i].AddComponent<Rigidbody>();

            }

            int n = 0;
            jointObjs[n++].name = "SpineBase";
            jointObjs[n++].name = "SpineMid";
            jointObjs[n++].name = "Neck";
            jointObjs[n++].name = "Head";
            jointObjs[n++].name = "ShoulderLeft";
            jointObjs[n++].name = "ElbowLeft";
            jointObjs[n++].name = "WristLeft";
            jointObjs[n++].name = "HandLeft";
            jointObjs[n++].name = "ShoulderRight";
            jointObjs[n++].name = "ElbowRight";
            jointObjs[n++].name = "WristRight";
            jointObjs[n++].name = "HandRight";
            jointObjs[n++].name = "HipLeft";
            jointObjs[n++].name = "KneeLeft";
            jointObjs[n++].name = "AnkleLeft";
            jointObjs[n++].name = "FootLeft";
            jointObjs[n++].name = "HipRight";
            jointObjs[n++].name = "KneeRight";
            jointObjs[n++].name = "AnkleRight";
            jointObjs[n++].name = "FootRight";
            jointObjs[n++].name = "SpineShoulder";
            jointObjs[n++].name = "HandTipLeft";
            jointObjs[n++].name = "HandThumbLeft";
            jointObjs[n++].name = "HandTipRight";
            jointObjs[n++].name = "HandThumbRight";

        }

        public int fillData(Byte[] data, int offset)
        {

            Buffer.BlockCopy(data, offset, this.id, 0, sizeof(UInt64));
            offset += sizeof(UInt64);
            Buffer.BlockCopy(data, offset, this.jointPos, 0, sizeof(float) * 4 * NumJoints);
            offset += sizeof(int) * 4 * NumJoints;
            Buffer.BlockCopy(data, offset, this.jointRots, 0, sizeof(float) * 4 * NumJoints);
            offset += sizeof(float) * 4 * NumJoints;
            Buffer.BlockCopy(data, offset, this.hands, 0, sizeof(int) * 2);
            offset += sizeof(int) * 2;

            return offset;
        }

        public void UpdateJoints()
        {
            


            for (int i = 0; i < NumJoints; i++)
            {
                int JointOffset = ((int)i) * 4;
                GameObject body = jointObjs[i];
                //body.transform.position = new Vector3(this.jointPos[JointOffset + 0], this.jointPos[JointOffset + 1], this.jointPos[JointOffset + 2]) * JointOffsetScale + this.transform.position;
                //body.transform.rotation = new Quaternion(this.jointRots[JointOffset + 0], this.jointRots[JointOffset + 1], this.jointRots[JointOffset + 2], this.jointRots[JointOffset + 3]);

                //switch to using the parent to define some of this?
                body.transform.localPosition = new Vector3(this.jointPos[JointOffset + 0], this.jointPos[JointOffset + 1], this.jointPos[JointOffset + 2]) * JointOffsetScale;
                body.transform.localRotation = new Quaternion(this.jointRots[JointOffset + 0], this.jointRots[JointOffset + 1], this.jointRots[JointOffset + 2], this.jointRots[JointOffset + 3]);


                if (this.jointPos[JointOffset + 3] == 0)
                    body.GetComponent<Renderer>().material.color = Color.red;
                else if (this.jointPos[JointOffset + 3] == 1)
                    body.GetComponent<Renderer>().material.color = Color.blue;
                else if (this.jointPos[JointOffset + 3] == 2)
                    body.GetComponent<Renderer>().material.color = Color.magenta;
            }
            //Debug.Log ("Set pos" + Name +" J:" + JointOffset + ": " + body.transform.position + " state:" + jointPos [JointOffset + 3]);

        }



    };



    //position of the kinect
    void OnGUI()
    {
        //	GUI.Label (new Rect (10, 50, 100, 20), "kin:"+ kinectOffset.y);
    }


    // Use this for initialization
    void Start()
    {

        for (int i = 0; i < MaxNumPlayers; i++)
        {
            KinectBodyContainers[i] = new KinectBody();
        }


        Debug.Log("Starting UDP on port" + receivePort);
        try
        {
            receivingUdpClient = new UdpClient(receivePort);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        // start the thread for receiving signals
        thread = new Thread(new ThreadStart(ReceiveDataBytes));
        thread.Start();
        // debug
        Debug.Log("Thread started");
       

    }

    void UpdateBodies()
    {

        for (int i = 0; i < MaxNumPlayers; i++)
        {
            GameObject g = GameObject.Find("player_" + KinectBodyContainers[i].id[0]);
            if (g)
            {

                KinectBody kb = g.GetComponent<KinectBody>();
                
                if(kb)
                {
                    kb.Copy(KinectBodyContainers[i]);
                    kb.UpdateJoints();

                }


            }
            //if we didn't find it, lets make a new game object
            else
            {
                if (KinectBodyContainers[i].id[0] != 0)
                {
                    g = new GameObject("player_" + KinectBodyContainers[i].id[0]);
                    g.transform.parent = this.transform;
                    //reset tranforms?
                    g.transform.localPosition = new Vector3(0, 0, 0);
                    g.transform.localRotation = new Quaternion();
                    g.transform.localScale = new Vector3(1, 1, 1);
                    KinectBody kb = g.AddComponent<KinectBody>();
                    kb.SetupBody(this);
                    Players.Add(g);
                }
            }

        }


        //now cleanup
        Transform[] allChildren = this.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            GameObject g = child.gameObject;
           
            if (g)
            {

                
                KinectBody kb = g.GetComponent<KinectBody>();
                if (kb)
                {
                   // Debug.Log(g.name + ":" + (Time.time - kb.updateTime));

                    if (Time.time - kb.updateTime > RemovalTime)
                    {
                        Destroy(g);
                    }
                }
            }

        }
     

    }

    void ReceiveDataBytes()
    {
        while (!exit)
        {
            //Debug.Log ("Threading inside while");
            // NOTE!: This blocks execution until a new message is received
            Byte[] receivedBytes = receivingUdpClient.Receive(ref remoteIpEndPoint);

            int offset = 0;
            int bodyNum = 0;

            while (offset < receivedBytes.Length)
            {
                
                offset = KinectBodyContainers[bodyNum].fillData(receivedBytes, offset);
                bodyNum++;

                if (bodyNum >= MaxNumPlayers)
                    break;

            }
            
            Thread.Sleep(0);
        }
    }
    
    
    void Update()
    {
      
        UpdateBodies();
        
    }

    void CloseClient()
    {
        exit = true;
        thread.Abort();
        receivingUdpClient.Close();
    }
    void OnApplicationQuit()
    {
        CloseClient();
    }
}