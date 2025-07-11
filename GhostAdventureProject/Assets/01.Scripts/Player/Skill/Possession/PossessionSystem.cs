using System.Collections;
using UnityEngine;

public class PossessionSystem : MonoBehaviour
{
    // 싱글톤
    public static PossessionSystem Instance { get; private set; }

    [SerializeField] private GameObject scanPanel;
    [SerializeField] private BasePossessable currentTarget; // 디버깅용
    private BasePossessable obssessingTarget;
    public BasePossessable CurrentTarget => currentTarget;

    private PlayerController Player => GameManager.Instance.PlayerController;
    public bool canMove { get; set; } = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // HideArea는 무시하고 빙의 가능 오브젝트만 처리
        if (other.CompareTag("HideArea"))
        {
            return; // HideArea는 PlayerHide.cs에서 처리하도록 무시
        }

        Debug.Log($"트리거 충돌: {other.name}");
        var possessionObject = other.GetComponent<BasePossessable>();
        if (possessionObject != null)
        {
            SetInteractTarget(possessionObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // HideArea는 무시하고 빙의 가능 오브젝트만 처리
        if (other.CompareTag("HideArea"))
        {
            return; // HideArea는 PlayerHide.cs에서 처리하도록 무시
        }

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

        obssessingTarget = currentTarget;

        switch (obssessingTarget.tag)
        {
            case "Animal":
                SoulEnergySystem.Instance.Consume(2);
                break;
            case "Cat":
                // 고양이는 풀 충전
                Debug.Log("고양이덕에 풀충전입니다옹");
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

        Debug.Log($"{obssessingTarget}에게 빙의 시도");
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

        if (obssessingTarget != null)
        {
            // ex) 애니메이션 끝나고 확대
            obssessingTarget.isPossessed = true;
            obssessingTarget.OnPossessionEnterComplete();
            Debug.Log($"빙의 애니메이션 종료 / 대상: {obssessingTarget.name}");
        }
        else
        {
            Debug.LogWarning("빙의 대상이 설정되지 않아서 이벤트가 호출되지 않았어요!");
        }
    }

    public void OnPossessionOutAnimationComplete() // 빙의 해제 애니메이션 후 이벤트
    {
        PossessionStateManager.Instance.PossessionOutAnimationComplete();
    }
}