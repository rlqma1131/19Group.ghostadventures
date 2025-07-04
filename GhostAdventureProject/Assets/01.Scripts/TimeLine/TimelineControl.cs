using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineControl : MonoBehaviour
{
    public PlayableDirector director;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {


            CloseScene(); //S키를 누르면 연출씬 닫기
        }
    }
    void Awake()
    {
        // timeScale 0에서도 재생되도록 설정
        director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

    }
    public void PauseTimeline()
    {
        Debug.Log("타임라인 일시정지");
        director.Pause();
    }

    public void ResumeTimeline()
    {

        Debug.Log("타임라인 재생");
        director.Play();
    }
    public void CloseScene()
    {
        string currentSceneName = gameObject.scene.name; //연출되고있는 씬이름 저장
        Time.timeScale = 1;
        SceneManager.UnloadSceneAsync(currentSceneName); //연출씬 닫고 원래 씬 이동


    }

    public void MoveScene(string Scenename)
    {
        // 타임라인이 종료되면 씬 이동
        SceneManager.LoadScene(Scenename);
    }

}
