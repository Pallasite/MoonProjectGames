using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;

public class ChangeClip : MonoBehaviour
{

    public VideoPlayer[] videoPlayer;
    public VideoClip[] clips;
    public int clipIndex = 0;

    void Prepared(VideoPlayer player)
    {
        player.Play();
        this.GetComponent<Renderer>().material.mainTexture = player.texture;// videoPlayer[clipIndex].texture;
        Debug.Log(player);
    }

    void SetClip(int index)
    {
        videoPlayer[clipIndex].Pause();
        videoPlayer[clipIndex].frame = 0;

        clipIndex = (index) % clips.Length;
        videoPlayer[clipIndex].frame = 0;
         videoPlayer[clipIndex].Prepare();
        videoPlayer[clipIndex].prepareCompleted += Prepared;


      


    }

    // Start is called before the first frame update
    void Start()
    {
        Array.Resize<VideoPlayer>(ref videoPlayer,clips.Length);

        for (int i = 0; i < clips.Length; i++)
        {
            GameObject g = new GameObject();
            g.name = "VideoClip" + i;
            g.transform.parent = this.transform;
            videoPlayer[i] = g.AddComponent<VideoPlayer>();
            Debug.Log(videoPlayer[i]);
            videoPlayer[i].clip = clips[i];
            //videoPlayer[i].Prepare();
          
            videoPlayer[i].playOnAwake = false;
            videoPlayer[i].Stop();
        }


        SetClip(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
         
            SetClip(clipIndex+1);
        }
    }
}
