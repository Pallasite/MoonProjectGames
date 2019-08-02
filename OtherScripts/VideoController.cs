using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class VideoController : MonoBehaviour
{
    public float StartTime;
    public float[] PauseTimes;
    private int PauseIndex = 0;

    private VideoPlayer videoPlayer;
    public TMPro.TextMeshPro tm;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = this.GetComponent<VideoPlayer>();
        videoPlayer.time = StartTime;
        videoPlayer.Prepare();
        videoPlayer.Play();

        PauseIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseTimes.Length > PauseIndex)
        if (videoPlayer.time  > PauseTimes[PauseIndex])
        {
                videoPlayer.Pause();
                PauseIndex++;
        }

        tm.text = "time:" + videoPlayer.time;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Resume();
        }
    }

   public void Resume()
    {
        if (videoPlayer.isPaused)
            videoPlayer.Play();

    }
}

