using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlayerCollider : MonoBehaviour
{
    public VideoController videoController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        videoController.Resume();
    }
}
