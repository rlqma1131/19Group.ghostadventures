using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TimelineControl : MonoBehaviour
{
    public PlayableDirector director;
    [SerializeField] Image skip;
    [SerializeField] Image space1;
    [SerializeField] Image space2;

    private bool isHolding = false;
    private Coroutine flashingCoroutine;

    private float skipTimer = 0f; // S 키를 누른 시간을 측정하는 타이머
    private const float SKIP_DURATION = 3.0f; // 스킵에 필요한 시간 (3초)
    private void Update()
    {

        if (Input.GetKey(KeyCode.Space))
        {
            isHolding = true;
            // 타이머 시간 증가 (UnscaledDeltaTime 사용으로 Time.timeScale 영향 없음)
            skipTimer += Time.unscaledDeltaTime;

            // 이미지의 fillAmount를 1에서 0으로 변경
            if (skip != null)
            {
                skip.fillAmount = 1.0f - (skipTimer / SKIP_DURATION); 
            }

            // 타이머가 3초를 넘으면 씬 닫기
            if (skipTimer >= SKIP_DURATION)
            {
                CloseScene();
            }
            ShowImage2Only();
        }


        if (Input.GetKeyUp(KeyCode.Space))
        {
            // 타이머와 이미지 fillAmount 초기화
            skipTimer = 0f;
            isHolding = false;
            if (skip != null)
            {
                skip.fillAmount = 1f;
            }
            StopCoroutine(flashingCoroutine);
            flashingCoroutine = StartCoroutine(FlashImages());
        }
    }
    void Awake()
    {
        if (director != null)
        {
        director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
       
        }
        // timeScale 0에서도 재생되도록 설정
        if (skip != null)
        {
            skip.fillAmount = 1f;
        }

    }

    private void Start()
    {
        flashingCoroutine = StartCoroutine(FlashImages());

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
        UIManager.Instance.PlayModeUI_OpenAll(); // 플레이모드 UI 열기
        PossessionSystem.Instance.CanMove = true; // 플레이어 이동 가능하게 설정
        SceneManager.UnloadSceneAsync(currentSceneName); //연출씬 닫고 원래 씬 이동
    }

    private IEnumerator FlashImages()
    {
        while (!isHolding)
        {
            space1.enabled = true;
            space2.enabled = false;
            yield return new WaitForSeconds(0.7f);

            space1.enabled = false;
            space2.enabled = true;
            yield return new WaitForSeconds(0.7f);
        }
    }

    private void ShowImage2Only()
    {
        if (flashingCoroutine != null)
            StopCoroutine(flashingCoroutine);

        space1.enabled = false;
        space2.enabled = true;
    }
}
