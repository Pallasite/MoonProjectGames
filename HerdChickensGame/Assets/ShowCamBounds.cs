using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCamBounds : MonoBehaviour
{
    void OnDrawGizmos()
    {
        float verticalHeightSeen = Camera.main.orthographicSize * 2.0f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3((verticalHeightSeen * Camera.main.aspect), verticalHeightSeen, 0));
    }

    // Start is called before the first frame update
    void Start()
    {
        void OnDrawGizmos()
        {
            float verticalHeightSeen = Camera.main.orthographicSize * 2.0f;
            float verticalWidthSeen = verticalHeightSeen * Camera.main.aspect;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(verticalWidthSeen, verticalHeightSeen, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        void OnDrawGizmos()
        {
            float verticalHeightSeen = Camera.main.orthographicSize * 2.0f;
            float verticalWidthSeen = verticalHeightSeen * Camera.main.aspect;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(verticalWidthSeen, verticalHeightSeen, 0));
        }

    }
}
