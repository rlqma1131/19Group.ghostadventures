using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessionSystem : Singleton<PossessionSystem>
{
    private PlayerController Player => GameManager.Instance.PlayerController;

    // 디버깅용
    [SerializeField] private BasePossessable currentTarget;
    public BasePossessable CurrentTarget => currentTarget;

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
        SoulEnergySystem.Instance.Consume(3);
        RequestPossession();
        return true;
    }
    public void RequestPossession()
    {
        Debug.Log($"{name} 빙의 시도 - QTE 호출");
        PossessionQTESystem.Instance.StartQTE();
    }

    public void SetInteractTarget(BasePossessable target)
    {
        if (!target.IsCompleted)
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
