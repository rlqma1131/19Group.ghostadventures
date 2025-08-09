using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
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

        //이어하기 모드로 스폰하도록 GM에 전달
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
}
