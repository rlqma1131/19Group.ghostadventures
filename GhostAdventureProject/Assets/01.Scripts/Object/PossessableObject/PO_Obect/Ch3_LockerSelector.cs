using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ch3_LockerSelector : MonoBehaviour
{
    public int RemainingOpens = 2;
    [SerializeField] private GameObject b1fDoor;
    public bool IsSolved { get; private set; } = false;
    
    [Header("정답 시 등장 오브젝트 연출")]
    [SerializeField] private Transform rewardObject;   // 기억조각 오브젝트
    [SerializeField] private Transform targetPoint;    // 플레이어 앞 위치
    [SerializeField] private float dropDuration = 2f;
    
    [Header("카메라 이동 관련")]
    [SerializeField] private Cinemachine.CinemachineVirtualCamera playerCam;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera rewardCam;

    [SerializeField] private float cameraReturnDelay = 2f;

    public void OnCorrectBodySelected()
    {
        b1fDoor.SetActive(true);
        IsSolved = true;

        var lockers = FindObjectsOfType<Ch3_Locker>();
        foreach (var locker in lockers)
        {
            locker.SetActivateState(false);
            if (!locker.IsCorrectBody)
            {
                locker.TryClose();
            }
        }

        StartCoroutine(HandleRewardSequence());
    }

    private IEnumerator HandleRewardSequence()
    {
        PossessionSystem.Instance.CanMove = false;

        // 빙의 해제 대기
        yield return new WaitForSeconds(2f);

        // 카메라 전환
        rewardCam.Priority = 20;
        playerCam.Priority = 10;

        // 보상 드랍 연출
        yield return new WaitForSeconds(0.5f);
        PlayRewardDrop();

        // 카메라 복귀
        yield return new WaitForSeconds(cameraReturnDelay + dropDuration);
        rewardCam.Priority = 10;
        playerCam.Priority = 20;

        PossessionSystem.Instance.CanMove = true;
    }

    private void PlayRewardDrop()
    {
        SpriteRenderer spriteRenderer = rewardObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            startColor.a = 0;
            spriteRenderer.color = startColor;

            rewardObject.gameObject.SetActive(true);
            rewardObject.position = new Vector3(
                targetPoint.position.x,
                targetPoint.position.y + 5f,
                targetPoint.position.z
            );

            rewardObject.DOMove(targetPoint.position, dropDuration)
                        .SetEase(Ease.OutBounce);

            spriteRenderer.DOFade(1f, dropDuration);
        }
    }

    public void OnWrongBodySelected()
    {
        if (RemainingOpens <= 0)
        {
            // 실패 처리
            Debug.Log("패널티 발생");
            StartCoroutine(ResetLockersAfterPenalty());
        }
    }

    private IEnumerator ResetLockersAfterPenalty()
    {
        // 패널티 연출 (사신 등장, 알람, 이펙트 등)
        yield return new WaitForSeconds(3f);

        RemainingOpens = 2; // 기회 리셋

        var lockers = FindObjectsOfType<Ch3_Locker>();
        foreach (var locker in lockers)
        {
            locker.SetActivateState(true);
            locker.TryClose(); // 혹시 열린 문 닫기
        }
    }
    
    // public bool HasAllClues()
    // {
    //     // 단서 획득 여부 확인 로직
    // }
}
