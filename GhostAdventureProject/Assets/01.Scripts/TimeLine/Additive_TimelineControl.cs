using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using _01.Scripts.Player;
using static _01.Scripts.Utilities.Timer;

//타임라인은 UnscaledGameTime으로 → 게임 멈춰도 정상 재생

//스페이스바 3초 유지 → 컷신 씬 Unload 후 원래 씬 복귀

//UI, 적 AI, 플레이어 컨트롤 정상화

// 메모리 조각 스캔 완료 처리 + 팝업 표시

//Additive 모드 컷씬에서 타임라인 제어
public class Additive_TimelineControl : MonoBehaviour
{
    public PlayableDirector director;
    [SerializeField] Image skip;
    [SerializeField] Image space1;
    [SerializeField] Image space2;

    private bool isHolding = false;
    private Coroutine flashingCoroutine;

    private float skipTimer = 0f; // S 키를 누른 시간을 측정하는 타이머
    private const float SKIP_DURATION = 3.0f; // 스킵에 필요한 시간 (3초)
    [SerializeField] private string prompt;
    private LoadSceneMode currentLoadMode = LoadSceneMode.Single;

    private MemoryScan memoryScan;
    CountdownTimer timer;
    Player player;

    void Awake() {
        if (director != null) {
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            // Time.timeScale = 0 이어도 타임라인 재생되도록 설정
        }
        // timeScale 0에서도 재생되도록 설정
        if (skip != null) {
            skip.fillAmount = 1f;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start() {
        player = GameManager.Instance.Player;
        memoryScan = player.MemoryScan;
        // 스킵용 카운트다운 타이머
        timer = new CountdownTimer(SKIP_DURATION);
        timer.OnTimerStart += () => {
            isHolding = true;
            ShowImage2Only();
        };
        timer.OnTimerStop += () => {
            // Basic initialization
            isHolding = false;
            if (skip) skip.fillAmount = 1f;
            
            // Event Condition => Did the user hold the button long enough?
            if (timer.IsFinished) {
                if (currentLoadMode != LoadSceneMode.Additive) return;
                CloseScene();
                // GoScene(nextSceneName);
            }
            else {
                flashingCoroutine = StartCoroutine(FlashImages());
            }
        };
        
        flashingCoroutine = StartCoroutine(FlashImages());
    }
    
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            timer.Start();
        }
        if (Input.GetKey(KeyCode.Space)) {
            // Time passes when the key is held
            timer.Tick(Time.unscaledDeltaTime);

            // 이미지의 fillAmount를 1에서 0으로 변경
            if (skip) {
                skip.fillAmount = timer.Progress; 
            }
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            timer.Stop();
        }
    }

    public void PauseTimeline() {
        Debug.Log("타임라인 일시정지");
        director.Pause();
    }

    public void ResumeTimeline() {
        Debug.Log("타임라인 재생");
        director.Resume();
    }

    //Additive 씬 닫기 + 원래 씬 복귀
    public void CloseScene() {
        Debug.Log("씬 닫기");
        
        string currentSceneName = gameObject.scene.name; //연출되고있는 씬이름 저장
        Time.timeScale = 1;
        UIManager.Instance.PlayModeUI_OpenAll(); // 플레이모드 UI 열기
        EnemyAI.ResumeAllEnemies();
        player.PossessionSystem.CanMove = true; // 플레이어 이동 가능하게 설정
        SceneManager.UnloadSceneAsync(currentSceneName); //연출씬 닫고 원래 씬 이동

        // 이전 BGM 복원
        SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);
        SoundManager.Instance.RestoreLastBGM(1f);
        // 안내 문구 표시
        if (prompt != null) {
            UIManager.Instance.PromptUI.ShowPrompt(prompt, 3f);
        }
        // 메모리 조각 스캔 완료 처리
        if (memoryScan.CurrentMemoryFragment) {
            memoryScan.CurrentMemoryFragment.AfterScan();
            UIManager.Instance.NoticePopupUI.FadeInAndOut($"※ 기억조각 저장 됨 - [{memoryScan.CurrentMemoryFragment.data.memoryTitle}]");
        }
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
        if (flashingCoroutine != null) {
            StopCoroutine(flashingCoroutine);
            flashingCoroutine = null;
        }

        space1.enabled = false;
        space2.enabled = true;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        currentLoadMode = mode; // 로드 모드 저장
    }
}
