using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Ch1 차고 문 앞에 콜라이더 트리거 설치 필요
/// </summary>
public class Ch1_GarageEvent : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1장 단서가 모두 수집되었는지 확인
        if (!ChapterEndingManager.Instance.AllCh1CluesCollected())
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // 꼬마유령 등장
            // "내 이름을 불러줘"
            // 이름 입력 창 띄우기
            // 맞 == 진행
            // 틀 == "..." 재도전 or 나가기
        }
    }
}

