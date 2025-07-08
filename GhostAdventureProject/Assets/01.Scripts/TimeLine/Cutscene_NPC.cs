using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class Cutscene_NPC : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    bool isCutscenePlaying = false;






    private void Play_NPCscene()
    {


        if (director != null)
        {
            director.Play();
            isCutscenePlaying = true;
        }
        else
        {
            Debug.LogError("PlayableDirector is not assigned or missing.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCutscenePlaying)
        {
            Play_NPCscene();
        }
    }
}
