using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class Ch3_MirrorRoomManager : MonoBehaviour
{
    [SerializeField] private List<Ch3_MirrorObj> differences;
    [SerializeField] private GameObject mirrorRoom;
    [SerializeField] private Transform playerMovePoint;

    private bool puzzleCleared = false;
    private GameObject player;

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    public void OnDifferenceFound(Ch3_MirrorObj obj)
    {
        if (puzzleCleared) return;

        if (differences.All(d => d.isFound))
        {
            puzzleCleared = true;
            StartCoroutine(ClearPuzzleRoutine());
        }
    }
    
    private IEnumerator ClearPuzzleRoutine()
    {
        if (player == null || playerMovePoint == null) yield break;

        SpriteRenderer[] playerSprites = player.GetComponentsInChildren<SpriteRenderer>();

        // 1. DOTween 페이드 아웃
        foreach (var sr in playerSprites)
        {
            sr.DOFade(0f, 0.4f);
        }
        yield return new WaitForSeconds(0.5f);

        // 2. 위치 이동
        player.transform.position = playerMovePoint.position;

        // 3. DOTween 페이드 인
        foreach (var sr in playerSprites)
        {
            sr.DOFade(1f, 0.4f);
        }
        yield return new WaitForSeconds(0.5f);

        // 4. mirrorRoom 내부 SpriteRenderer 페이드 아웃
        SpriteRenderer[] mirrorSprites = mirrorRoom.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var sr in mirrorSprites)
        {
            sr.DOFade(0f, 0.5f);
        }

        yield return new WaitForSeconds(0.6f);

        // 5. mirrorRoom 비활성화
        mirrorRoom.SetActive(false);

        // 6. 퍼즐 완료 통보
        Ch3_MirrorPuzzleManager.Instance.NotifyRoomCleared(mirrorRoom);
    }

    // 오답 처리용 함수
    public void OnWrongObjectReleased()
    {
        Debug.Log("틀린 오브젝트에 빙의 해제됨 - 패널티 실행");

        // 여기서 패널티 로직 추가 (예: 깜빡임, 시간 감소, 사운드 등)
    }
}
