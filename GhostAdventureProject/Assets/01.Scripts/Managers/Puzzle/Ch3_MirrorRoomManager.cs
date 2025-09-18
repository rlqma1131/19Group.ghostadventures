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

    private void Start() {
        player = GameManager.Instance.PlayerObj;
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
        yield return new WaitForSecondsRealtime(0.5f);
        
        player.transform.position = playerMovePoint.position;
        
        foreach (var sr in playerSprites)
        {
            sr.DOFade(1f, 0.4f);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        
        SpriteRenderer[] mirrorSprites = mirrorRoom.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var sr in mirrorSprites)
        {
            sr.DOFade(0f, 0.5f);
        }

        yield return new WaitForSecondsRealtime(0.6f);
        
        mirrorRoom.SetActive(false);
        
        Ch3_MirrorPuzzleManager.Instance.NotifyRoomCleared();
    }

    // 오답 처리용 함수
    public void OnWrongObjectReleased()
    {
        UIManager.Instance.PromptUI.ShowPrompt("이게 아니야...", 2f);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Ch3_MirrorPuzzleManager.Instance.NotifyFirstRoomEntered();
        }
    }
}
