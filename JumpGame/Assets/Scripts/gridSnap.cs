using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gridSnap : MonoBehaviour
{
    [Header("")]
    [Header("10 willSnapTo 0.1")]
    [Header("5 willSnapTo 0.2")]
    [Header("4 willSnapTo 0.25")]

    [Header("2 willSnapTo 0.5")]
    [SerializeField]
    private float position_snap = 5;
    [SerializeField]
    private float size_snap = 5;
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) // check if game is playing
        {

            if (transform.hasChanged)
            {
                transform.hasChanged = false;
            }
            else
            {
                transform.localScale = new Vector2(Mathf.Round(transform.localScale.x * size_snap) / size_snap, Mathf.Round(transform.localScale.y * size_snap) / size_snap);
                transform.position = new Vector2(Mathf.Round(transform.position.x * position_snap) / position_snap, Mathf.Round(transform.position.y * position_snap) / position_snap);
            }
        }
    }
}
