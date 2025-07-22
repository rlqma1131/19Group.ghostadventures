using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MoveScene : MonoBehaviour
{
    [SerializeField] Image skip; // 스킵 진행 상태를 표시할 이미지

    private float skipTimer = 0f; // S 키를 누른 시간을 측정하는 타이머
    private const float SKIP_DURATION = 3.0f; // 스킵에 필요한 시간 (3초)
    private void Awake()
    {
        skip.fillAmount = 1f;

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.S))
        {
            Debug.Log("S 키를 눌렀습니다. 타임라인 스킵 시작");
            skipTimer += Time.unscaledDeltaTime;
            skip.fillAmount = 1.0f - (skipTimer / SKIP_DURATION);

            if (skipTimer >= SKIP_DURATION)
            {
            GoScene("Ch01_House"); 
                
            }
            if(UIManager.Instance != null)
            {
                UIManager.Instance.PlayModeUI_OpenAll();
            }
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            // 타이머와 이미지 fillAmount 초기화
            skipTimer = 0f;
            if (skip != null)
            {
                skip.fillAmount = 1f;
            }
        }

    }

    public void GoScene(string Scenename)
    {
        // 타임라인이 종료되면 씬 이동
        SceneManager.LoadScene(Scenename);
    }
}
