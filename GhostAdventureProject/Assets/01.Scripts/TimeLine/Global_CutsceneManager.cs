using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.SceneManagement;

public class Global_CutsceneManager : Singleton<Global_CutsceneManager>
{


    // 기억조각 스캔시 화면 점점 어두워지는 효과
    public PlayableDirector director; // 컷신(타임라인)을 재생하는 PlayableDirector
    public GameObject fadePanel; // 페이드 인/아웃을 위한 패널


    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    //페이드인 재생
    public IEnumerator PlayCutscene()
    {

        Debug.Log("컷신 재생 시작");
        bool isDone = false;

        void OnPlayableDirectorStopped(PlayableDirector obj)
        {
            if (obj == director)
            {
                Debug.Log("컷신 재생 완료");
                isDone = true;
                director.stopped -= OnPlayableDirectorStopped;
            }
        }

        director.stopped += OnPlayableDirectorStopped;

        director.Play();

        yield return new WaitUntil(() => isDone);

        fadePanel.SetActive(false);
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"{scene.name} 씬 언로드됨");
        if (fadePanel != null)
            fadePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
