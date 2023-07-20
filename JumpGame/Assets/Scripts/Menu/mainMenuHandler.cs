using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainMenuHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    public void do_play()
    {
        StartCoroutine(playDelayCoroutine());
    }

    private IEnumerator playDelayCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        player.SetActive(true);
    }
}
