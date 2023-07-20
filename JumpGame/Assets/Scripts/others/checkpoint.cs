using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    [SerializeField]
    Sprite activatedCheckpoint;

    private AudioSource thisAudio;

    private void Awake()
    {
        thisAudio = GetComponent<AudioSource>();
    }

    internal void do_activateCheckpoint()
    {
        GetComponent<SpriteRenderer>().sprite = activatedCheckpoint;
        thisAudio.Play();
        GetComponent<BoxCollider2D>().enabled = false;
    }
}
