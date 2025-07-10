using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Ch1 차고 문 앞에 콜라이더 트리거 설치 필요
/// </summary>
public class Ch1_GarageEvent : MonoBehaviour
{
    private Ch1_MemoryPositive_01_TeddyBear bear;

    void Start()
    {
        bear = GetComponent<Ch1_MemoryPositive_01_TeddyBear>();    
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1장 단서가 모두 수집되었는지 확인
        if (!ChapterEndingManager.Instance.AllCh1CluesCollected())
            return;

        // 1장 단서 모두 모이고 충돌 시 이벤트 발생
        if (collision.gameObject.CompareTag("Player"))
        {
            PossessionSystem.Instance.canMove = false;

            // 꼬마유령 등장
            // 깜짝놀래키기
            // 이름 입력 창 띄우기

            // if(맞) == 진행 / 기억조각 스캔 가능
            bear.ActivateTeddyBear();
            PossessionSystem.Instance.canMove = true;

            // if(틀) == "..." 재도전 or 나가기
            PossessionSystem.Instance.canMove = true;
        }
    }
}

