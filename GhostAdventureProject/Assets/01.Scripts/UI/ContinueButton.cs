using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
    public void OnClickContinue()
    {
        SaveData data = SaveManager.LoadGame();
        if (data != null)
        {
            GameManager.Instance.SetPendingLoad(data);
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            Debug.Log("저장된 데이터가 없습니다");
        }
    }

    //private IEnumerator LoadSceneAndSpawn(SaveData data)
    //{
    //    AsyncOperation op = SceneManager.LoadSceneAsync(data.sceneName);
    //    yield return new WaitUntil(() => op.isDone);

    //    Debug.Log($"씬 로드 완료: {data.sceneName}, 플레이어 스폰");
    //    GameManager.Instance.SpawnPlayer(true); // 저장된 위치에서 스폰
    //}
}
