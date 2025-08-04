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

        SpriteRenderer[] playerSprites = player.GetComponents<SpriteRenderer>();
        
        foreach (var sr in playerSprites)
        {
            sr.DOFade(0f, 0.4f);
        }
        yield return new WaitForSeconds(0.5f);
        
        player.transform.position = playerMovePoint.position;
        
        foreach (var sr in playerSprites)
        {
            sr.DOFade(1f, 0.4f);
        }
        yield return new WaitForSeconds(0.5f);
        
        SpriteRenderer[] mirrorSprites = mirrorRoom.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var sr in mirrorSprites)
        {
            sr.DOFade(0f, 0.5f);
        }

        yield return new WaitForSeconds(0.6f);
        
        mirrorRoom.SetActive(false);
        
        Ch3_MirrorPuzzleManager.Instance.NotifyRoomCleared();
    }

    // 오답 처리용 함수
    public void OnWrongObjectReleased()
    {
        Debug.Log("틀린 오브젝트에 빙의 해제됨 - 패널티 실행");

        // 여기서 패널티 로직 추가 (예: 깜빡임, 시간 감소, 사운드 등)
    }
}
