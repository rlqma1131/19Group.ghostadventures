using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessionSystem : Singleton<PossessionSystem>
{
    [SerializeField] private GameObject scanPanel;
    [SerializeField] private BasePossessable currentTarget; // 디버깅용
    public BasePossessable CurrentTarget => currentTarget;

    private PlayerController Player => GameManager.Instance.PlayerController;
    public bool canMove { get; set; } = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"트리거 충돌: {other.name}");
        var possessionObject = other.GetComponent<BasePossessable>();
        if (possessionObject != null)
        {
            SetInteractTarget(possessionObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var possessionObject = other.GetComponent<BasePossessable>();
        if (possessionObject != null)
        {
            ClearInteractionTarget(possessionObject);
        }
    }
    public bool TryPossess()
    {
        if (!SoulEnergySystem.Instance.HasEnoughEnergy(3))
        {
            Debug.Log("Not enough energy");
            return false;
        }

        switch (currentTarget.tag)
        {
            case "Animal":
                SoulEnergySystem.Instance.Consume(2);
                break;
            case "Cat":
                // 고양이는 풀 충전
                SoulEnergySystem.Instance.RestoreAll();
                break;
            case "Person":
                // 사람 구현되면 피로도에 따라 소모량 조정
                SoulEnergySystem.Instance.Consume(3);
                break;
            default:
                SoulEnergySystem.Instance.Consume(3);
                break;
        }

        RequestPossession();
        return true;
    }
    public void RequestPossession()
    {
        Debug.Log($"{name} 빙의 시도 - QTE 호출");
        PossessionQTESystem.Instance.StartQTE();
    }

    // 빙의 가능 대상 설정
    public void SetInteractTarget(BasePossessable target)
    {
        if (!target.HasActivated)
            return;

        currentTarget = target;
        if (Player != null)
            Player.currentTarget = currentTarget;
    }

    public void ClearInteractionTarget(BasePossessable target)
    {
        if (currentTarget == target)
        {
            currentTarget = null;
            if (Player != null)
                Player.currentTarget = null;
        }
    }

    public void PlayPossessionInAnimation() // 빙의 시작 애니메이션
    {
        Debug.Log("빙의 시작 애니메이션 재생");
        canMove = false;
        Player.animator.SetTrigger("PossessIn");
    }

    public void StartPossessionOutSequence() // 빙의 해제 애니메이션
    {
        canMove = false;
        StartCoroutine(DelayedPossessionOutPlay());
    }

    private IEnumerator DelayedPossessionOutPlay()
    {
        yield return null; // 한 프레임 딜레이
        Player.animator.Play("Player_PossessionOut");
    }

    public void OnPossessionInAnimationComplete() // 빙의 시작 애니메이션 후 이벤트
    {
        PossessionStateManager.Instance.PossessionInAnimationComplete();
    }

    public void OnPossessionOutAnimationComplete() // 빙의 해제 애니메이션 후 이벤트
    {
        PossessionStateManager.Instance.PossessionOutAnimationComplete();
    }
}
