using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
    public void OnClickContinue()
    {
        SaveData data = SaveManager.LoadGame();
        if (data != null)
        {
            StartCoroutine(LoadSceneAndSpawn(data));
        }
        else
        {
            Debug.Log("저장된 데이터가 없습니다");
        }
    }

    private IEnumerator LoadSceneAndSpawn(SaveData data)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(data.sceneName);
        yield return new WaitUntil(() => op.isDone);

        GameManager.Instance.SpawnPlayer(true); // 저장된 위치에서 스폰
    }
}
