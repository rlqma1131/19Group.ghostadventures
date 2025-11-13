using System.Collections;
using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static _01.Scripts.Utilities.Timer;

// Additive 모드가 아닌 일반 컷신 씬에서
//  - 타임라인 종료 시 다음 씬 이동
//  - 스페이스바로 스킵 기능
public class Scene_To_Scene_MoveScene : MonoBehaviour
{
    [SerializeField] Image skip; // 스킵 진행 상태를 표시할 이미지
    [SerializeField] string nextSceneName; //이동할 씬 이름

    float skipTimer; 
    const float SKIP_DURATION = 2.0f; // 스킵에 필요한 시간 (3초)
    bool isSkipActive; // 스킵 활성화 여부

    // 스킵이미지 깜빡임
    [SerializeField] Image space1;
    [SerializeField] Image space2;


    LoadSceneMode currentLoadMode = LoadSceneMode.Single; // 현재 씬의 로드 모드 (Additive 방지용)
    bool isHolding;
    Coroutine flashingCoroutine; // 깜빡임 코루틴 저장
    Player player;
    CountdownTimer timer;

    void Awake() {
        if (skip != null)
            skip.fillAmount = 1f;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start() {
        player = GameManager.Instance.Player;
        
        timer = new CountdownTimer(SKIP_DURATION);
        timer.OnTimerStart += () => {
            isHolding = true;
            ShowImage2Only();
        };
        timer.OnTimerStop += () => {
            // Basic initialization
            isHolding = false;
            
            // Event Condition => Did the user hold the button long enough?
            if (timer.IsFinished) {
                if (skip) skip.fillAmount = 0f;
                GoScene(nextSceneName);
                if (currentLoadMode != LoadSceneMode.Additive) return;
              
                // GoScene(nextSceneName);
            }
            else {
                flashingCoroutine = StartCoroutine(FlashImages());
                if (skip) skip.fillAmount = 1f;
            }
        };
        
        flashingCoroutine = StartCoroutine(FlashImages());
        flashingCoroutine = StartCoroutine(FlashImages());
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            timer.Start();
        }

        if (isHolding) {
            timer.Tick(Time.unscaledDeltaTime);
            if (skip) { 
                skip.fillAmount = timer.Progress; 
            }
        }
        
        if (Input.GetKeyUp(KeyCode.Space)) {
            timer.Stop();
        }
    }

    public void GoScene(string Scenename) {
        // 타임라인이 종료되면 씬 이동
        GameManager.LoadThroughLoading(Scenename);


        Debug.Log("씬 이동: " + Scenename);

        if (GameManager.Instance.PlayerObj != null) {
            player.PossessionSystem.CanMove = true; // 플레이어 이동 가능하도록 설정
        }
        //}
    }

    void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded; //이벤트 해제
    }


    // 씬이 로드될 때 호출되는 함수 → 현재 씬이 Additive인지 체크
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        currentLoadMode = mode; // 로드 모드 저장
    }

    IEnumerator FlashImages() {
        while (!isHolding) {
            if (space1 != null) space1.enabled = true;
            if (space2 != null) space2.enabled = false;

            yield return new WaitForSeconds(0.7f);

            if (space1 != null) space1.enabled = false;
            if (space2 != null) space2.enabled = true;
            yield return new WaitForSeconds(0.7f);
        }
    }

    void ShowImage2Only() {
        if (flashingCoroutine != null)
            StopCoroutine(flashingCoroutine);

        if (space1 != null) space1.enabled = false;
        if (space2 != null) space2.enabled = true;
    }
}