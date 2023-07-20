using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraHandler : MonoBehaviour
{
    private Camera thisCam;
    private GameObject player;
    private Vector2 playerPos;

    [SerializeField]
    private bool introDone;

    private void Start()
    {
        player = GameObject.Find("Player");
        thisCam = GetComponent<Camera>();
        GetComponent<Camera>().aspect = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerPos = player.transform.position;

       if (introDone)
        {
            if (playerPos.x > transform.position.x + thisCam.orthographicSize)
            {
                shiftCam("right");
            }
            else if (playerPos.x < transform.position.x - thisCam.orthographicSize)
            {
                shiftCam("left");
            }
            else if (playerPos.y > transform.position.y + thisCam.orthographicSize)
            {
                shiftCam("up");
            }
            else if (playerPos.y < transform.position.y - thisCam.orthographicSize)
            {
                shiftCam("down");
            }
        }
    }

    private void shiftCam(string direction)
    {
        switch (direction)
        {
            case "left":
                transform.position = transform.position + (Vector3.left * thisCam.orthographicSize * 2);
                break;
            case "right":
                transform.position = transform.position + (Vector3.right * thisCam.orthographicSize * 2);
                break;
            case "up":
                transform.position = transform.position + (Vector3.up * thisCam.orthographicSize * 2);
                break;
            case "down":
                transform.position = transform.position + (Vector3.down * thisCam.orthographicSize * 2);
                break;
        }
    }

    private void OnDrawGizmos()
    {
        GetComponent<Camera>().aspect = 1;
    }
}
