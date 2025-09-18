using System.Collections;
using UnityEngine;

public class PossessionQTESystem : MonoBehaviour
{
    // 싱글톤
    public static PossessionQTESystem Instance { get; private set; }

    [SerializeField] QTEUI QTEUI;
    [SerializeField] QTEUI3 QTEUI3;

    public bool isRunning { get; private set; }

    void Awake() {
        if (Instance == null) { Instance = this; }
    }

    void Start() {
        if (QTEUI != null)
            QTEUI.gameObject.SetActive(false);
        if (QTEUI3 != null)
            QTEUI3.gameObject.SetActive(false);
    }

    public void StartQTE() {
        Time.timeScale = 0.3f;
        // UIManager연동되면 스캔 때 까만 배경 활성화
        isRunning = true;
        Debug.Log("Starting QTE");
        QTEUI.ShowQTEUI();
        EnemyAI.PauseAllEnemies();
    }

    public void StartQTE3() {
        Time.timeScale = 0.3f;
        // UIManager연동되면 스캔 때 까만 배경 활성화
        isRunning = true;
        Debug.Log("Starting QTE3");
        QTEUI3.ShowQTEUI3();
        EnemyAI.PauseAllEnemies();
    }

    public void HandleQTEResult(bool success) {
        isRunning = false;
        // UIManager연동되면 스캔 때 까만 배경 비활성화
        GameManager.Instance.Player.PossessionSystem.CanMove = true;

        ResetTimeScale();

        if (success) {
            Debug.Log("QTE succeeded");
            // ResetTimeScale();
            EnemyAI.ResumeAllEnemies();
            GameManager.Instance.Player.PossessionSystem.PossessedTarget?.OnQTESuccess();
            UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("빙의 성공!", 2f);
        }
        else {
            Debug.Log("QTE failed");
            EnemyAI.ResumeAllEnemies();
            StartCoroutine(DelayedFailure());
            UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("빙의 실패!", 2f);
        }
    }

    void ResetTimeScale() => Time.timeScale = 1f;

    IEnumerator DelayedFailure() {
        yield return new WaitForSeconds(0.05f);
        // ResetTimeScale();
        GameManager.Instance.Player.PossessionSystem.PossessedTarget?.OnQTEFailure();
    }
}