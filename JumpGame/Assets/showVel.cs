using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showVel : MonoBehaviour
{
    public float veloY;

    private void Update()
    {
        veloY = GetComponent<Rigidbody2D>().velocity.y;
    }
}
