using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene_MenuButtons : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] private GameObject optionWindow;
    [SerializeField] private GameObject credit;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    [Header("New Game Confirm UI")]
    [SerializeField] private GameObject newGameConfirmPanel;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private static readonly Color EnabledColor = Color.white;
    private static readonly Color DisabledColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private void OnEnable()
    {
        UpdateContinueButtonState();
    }

    // 포커스 돌아왔을 때(세이브 파일이 생겼을 수도 있으니) 한 번 더 안전하게
    private void OnApplicationFocus(bool focus)
    {
        if (focus) UpdateContinueButtonState();
    }

    void Awake()
    {
        // 새게임 버튼 셋업
        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(OnConfirmNewGame);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(CloseNewGameConfirm);
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            // ESC 키로 확인창 닫기
            if (newGameConfirmPanel != null && newGameConfirmPanel.activeSelf)
            {
                CloseNewGameConfirm();
            }
            else if (optionWindow != null && optionWindow.activeSelf)
            {
                optionWindow.SetActive(false);
            }
            else if (credit != null && credit.activeSelf)
            {
                credit.SetActive(false);
            }
        }
    }
    // 이어하기 버튼 셋업
    private void UpdateContinueButtonState()
    {
        bool hasSave = SaveManager.HasSaveFile();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;

            // 하위 텍스트 색상 변경 (UGUI Text / TMP_Text 모두 처리)
            var uguiText = continueButton.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (uguiText != null) uguiText.color = hasSave ? EnabledColor : DisabledColor;

            var tmpText = continueButton.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (tmpText != null) tmpText.color = hasSave ? EnabledColor : DisabledColor;
        }
    }

    public void OnClickNewGame()
    {
        // 저장 파일 있으면 경고 팝업 표시, 없으면 바로 시작
        if (SaveManager.HasSaveFile())
        {
            OpenNewGameConfirm();
        }
        else
        {
            StartNewGameImmediate();
        }
    }

    private void StartNewGameImmediate()
    {
        SceneManager.LoadScene("IntroScene_Real");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }

    public void OpenNewGameConfirmInChapterSelection(string chapter) {
        SaveManager.DeleteSave();
        ChapterEndingManager.Instance?.ResetAllAndNotify();

        MemoryData.Chapter ch = chapter switch {
            "1" => MemoryData.Chapter.Chapter1,
            "2" => MemoryData.Chapter.Chapter2,
            "3" => MemoryData.Chapter.Chapter3,
            "4" => MemoryData.Chapter.Chapter4,
            _ => MemoryData.Chapter.Exception
        };
        
        switch (ch) {
            case MemoryData.Chapter.Chapter1: StartInChapterOne(); break;
            case MemoryData.Chapter.Chapter2: StartInChapterTwo(); break;
            case MemoryData.Chapter.Chapter3: StartInChapterThree(); break;
            case MemoryData.Chapter.Chapter4: StartInChapterFour(); break;
            case MemoryData.Chapter.Exception:
            default: throw new ArgumentException("Incorrect Chapter Exception Occurred", nameof(chapter));
        }
    }

    void StartInChapterOne() {
        GameManager.Instance.ByPassEnabled = true;
        SceneManager.LoadScene("IntroScene_Real");
        if (UIManager.Instance != null) {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }

    void StartInChapterTwo() {
        GameManager.Instance.ByPassEnabled = true;
        SceneManager.LoadScene("00.Scenes/CutScene/Ch01_To_Ch02");
        if (UIManager.Instance != null) {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }

    void StartInChapterThree() {
        GameManager.Instance.ByPassEnabled = true;
        SceneManager.LoadScene("00.Scenes/CutScene/Ch02_To_Ch03");
        if (UIManager.Instance != null) {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }

    void StartInChapterFour() {
        GameManager.Instance.ByPassEnabled = true;
        SceneManager.LoadScene("00.Scenes/CutScene/Ch03_To_Ch04");
        if (UIManager.Instance != null) {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }

    private void OpenNewGameConfirm()
    {
        if (newGameConfirmPanel == null) { StartNewGameImmediate(); return; }
        newGameConfirmPanel.SetActive(true);

        // (선택) 메인 버튼들 비활성화해서 중복 입력 방지
        //if (continueButton != null) continueButton.interactable = false;
        //if (optionButton != null) optionButton.interactable = false;
    }

    private void CloseNewGameConfirm()
    {
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);

        // (선택) 메인 버튼 복구
        //if (continueButton != null) continueButton.interactable = true;
        //if (optionButton != null) optionButton.interactable = true;
    }

    private void OnConfirmNewGame()
    {
        // 저장 데이터 삭제
        SaveManager.DeleteSave();

        // 진행도 비우기
        ChapterEndingManager.Instance?.ResetAllAndNotify();

        // 0.1초 뒤에 UI 닫고 새 게임 시작
        StartCoroutine(DelayStartNewGame());
    }

    private IEnumerator DelayStartNewGame()
    {
        yield return new WaitForSeconds(0.1f);
        CloseNewGameConfirm();
        StartNewGameImmediate();
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
        if(optionWindow != null)
            optionWindow.SetActive(true);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickCredit()
    {
        if(credit != null)
            credit.gameObject.SetActive(true);
    }
    
    public void OpenURL()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSetE6cy2Iu6odXTSfW-ym8_2uxIw4b539wSyZo0Io8N3jNoeg/viewform?usp=dialog");
    }
}
