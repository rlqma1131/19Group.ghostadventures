using System.Collections;
using UnityEngine;

public class PossessionQTESystem : MonoBehaviour
{
    // 싱글톤
    public static PossessionQTESystem Instance { get; private set; }

    [SerializeField] private QTEUI QTEUI;
    public bool isRunning { get; private set; } = false;

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        QTEUI.gameObject.SetActive(false);
    }

    public void StartQTE()
    {
        Time.timeScale = 0.3f;
        // UIManager연동되면 스캔 때 까만 배경 활성화
        isRunning = true;
        Debug.Log("Starting QTE");
        QTEUI.ShowQTEUI();
    }

    public void HandleQTEResult(bool success)
    {
        isRunning = false;
        // UIManager연동되면 스캔 때 까만 배경 비활성화
        PossessionSystem.Instance.CanMove = true;
        if (success)
        {
            Debug.Log("QTE succeeded");
            ResetTimeScale();
            GameManager.Instance.PlayerController.currentTarget?.OnQTESuccess();
        }
        else
        {
            Debug.Log("QTE failed");
            StartCoroutine(DelayedFailure());
        }
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }

    private IEnumerator DelayedFailure()
    {
        yield return new WaitForSeconds(0.05f);
        ResetTimeScale();
        GameManager.Instance.PlayerController.currentTarget?.OnQTEFailure();
    }

    //public void OnQTESuccess()
    //{
    //    Debug.Log("QTE 성공 - 빙의 완료");

    //    PossessionSystem.Instance.CurrentTarget.isPossessed = true;
    //    PossessionStateManager.Instance.StartPossessionTransition();
    //}

    //public void OnQTEFailure()
    //{
    //    Debug.Log("QTE 실패 - 빙의 취소");
    //    PossessionSystem.Instance.CurrentTarget.isPossessed = false;
    //    SoulEnergySystem.Instance.Consume(1);
    //}
}
