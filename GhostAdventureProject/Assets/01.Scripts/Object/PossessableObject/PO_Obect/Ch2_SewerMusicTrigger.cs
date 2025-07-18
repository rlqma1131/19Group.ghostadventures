using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SewerMusicTrigger : MonoBehaviour
{
    private Ch2_SewerMusicPuzzle musicPuzzle;

    private void Awake()
    {
        musicPuzzle = FindObjectOfType<Ch2_SewerMusicPuzzle>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == GameManager.Instance.Player)
        {
            musicPuzzle?.StartPuzzle();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == GameManager.Instance.Player)
        {
            musicPuzzle?.StopPuzzle();
        }
    }
}
