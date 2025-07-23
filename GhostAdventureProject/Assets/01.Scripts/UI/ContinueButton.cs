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
}
