using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutsceneManager : Singleton<CutsceneManager>
{
    public GameObject fadePanelPrefab;       // 패널 프리팹
    public GameObject directorPrefab;        // 타임라인이 미리 연결된 디렉터 프리팹

    private GameObject fadePanelInstance;
    private PlayableDirector directorInstance;

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public IEnumerator PlayCutscene()
    {
        Debug.Log("컷신 재생 시작");

        if (fadePanelInstance == null)
            fadePanelInstance = Instantiate(fadePanelPrefab);
        fadePanelInstance.SetActive(true);

        if (directorInstance == null)
        {
            GameObject go = Instantiate(directorPrefab);
            directorInstance = go.GetComponent<PlayableDirector>();
        }

        bool isDone = false;

        void OnPlayableDirectorStopped(PlayableDirector obj)
        {
            if (obj == directorInstance)
            {
                Debug.Log("컷신 재생 완료");
                isDone = true;
                directorInstance.stopped -= OnPlayableDirectorStopped;
            }
        }

        directorInstance.stopped += OnPlayableDirectorStopped;
        directorInstance.Play();

        yield return new WaitUntil(() => isDone);

        if (fadePanelInstance != null)
            fadePanelInstance.SetActive(false);

        // 오브젝트 정리
        Destroy(fadePanelInstance);
        Destroy(directorInstance.gameObject);
        fadePanelInstance = null;
        directorInstance = null;
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"{scene.name} 씬 언로드됨");

        if (fadePanelInstance != null)
        {
            Destroy(fadePanelInstance);
            fadePanelInstance = null;
        }

        if (directorInstance != null)
        {
            Destroy(directorInstance.gameObject);
            directorInstance = null;
        }

        Time.timeScale = 1f;
    }
}
