using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene_MenuButtons : MonoBehaviour
{
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button optionBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button surveyBtn;

    private Text surveyText;

    //private void Awake()
    //{
    //    // onClick 이벤트 등록
    //    newGameBtn.onClick.AddListener(OnClickNewGame);
    //    continueBtn.onClick.AddListener(OnClickContinue);
    //    optionBtn.onClick.AddListener(OnClickOption);
    //    exitBtn.onClick.AddListener(OnClickExit);
    //    surveyBtn.onClick.AddListener(OpenURL);
    //}

    public void OnClickNewGame()
    {
        SceneManager.LoadScene("IntroScene_Real");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }

    public void OnClickContinue()
    {
        // 1) 저장파일 없는 경우 처리
        if (!SaveManager.HasSaveFile())
        {
            Debug.Log("[ContinueButton] 저장 데이터 없음");
            return;
        }

        // 2) 저장 데이터 로드
        var data = SaveManager.LoadGame();
        if (data == null) return;

        // 이어하기 모드로 스폰하도록 GM에 전달
        GameManager.Instance.SetPendingLoad(data);

        // 3) 저장된 씬 이름 가져오기
        string sceneName = SaveManager.GetLastSceneName();
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[ContinueButton] 저장된 씬 이름 없음");
            return;
        }

        // 4) 씬 로드 → GameManager에서 OnSceneLoaded가 플레이어 스폰
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickOption()
    {
        Debug.Log("옵션 창 열기");
        // 옵션 UI 열기 처리
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenURL()
    {
        surveyText = GetComponentInChildren<Text>();
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSetE6cy2Iu6odXTSfW-ym8_2uxIw4b539wSyZo0Io8N3jNoeg/viewform?usp=dialog");
        surveyText.text = "감사합니다!";
    }
}
