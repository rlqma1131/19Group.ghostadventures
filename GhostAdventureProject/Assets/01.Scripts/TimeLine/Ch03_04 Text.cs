using System.Collections;
using UnityEngine;
using TMPro;


//3챕에서 4챕 이동중 콜라이더 지나갈때 텍스트 띄우기 위한 스크립트
public class Ch03_04Text : MonoBehaviour
{
    bool isTextActive = false;  // 텍스트가 활성화되었는지 여부
    [SerializeField]private string textMessage = "";
    [SerializeField] private TextMeshProUGUI textUI;  // Canvas에 있는 TMP UI 텍스트 연결

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTextActive)  // Player 태그 확인
        {
            ShowMessage();
            isTextActive = true;  // 텍스트가 활성화됨
        }
    }

    public void ShowMessage()
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageCoroutine());
    }

    IEnumerator ShowMessageCoroutine()
    {
        textUI.text = textMessage;
        textUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        textUI.gameObject.SetActive(false);
    }
}
