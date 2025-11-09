using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
public class Ch04_Memory5_MoveScene : MonoBehaviour
{


    [SerializeField] private PlayableDirector director;

    private void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        director.stopped += OnDirectorStopped;
    }

    private void OnDestroy()
    {
        director.stopped -= OnDirectorStopped;
    }

    private void OnDirectorStopped(PlayableDirector dir)
    {
        
        SceneManager.LoadScene("End_분기");
        UIManager.Instance.PlayModeUI_CloseAll();
    }
}
