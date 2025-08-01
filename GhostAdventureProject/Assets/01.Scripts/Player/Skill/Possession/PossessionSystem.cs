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
    
    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

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
        obssessingTarget = currentTarget;

        switch (obssessingTarget.tag)
        {
            case "Scanner":
            case "Cat":
                break;
            
            case "Animal":
                if (!SoulEnergySystem.Instance.HasEnoughEnergy(2))
                {
                    UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다", 2f);
                    return false;
                }
                else
                {
                    SoulEnergySystem.Instance.Consume(2);
                }
                break;

            case "Person":
                // 사람 구현되면 피로도에 따라 소모량 조정
                if (!SoulEnergySystem.Instance.HasEnoughEnergy(1))
                {
                    UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다", 2f);
                    return false;
                }
                else
                {
                    PersonCondition condition = obssessingTarget.GetComponent<PersonConditionUI>().currentCondition;
                    if(condition == PersonCondition.Vital)
                    {
                        SoulEnergySystem.Instance.Consume(-1);
                        break;
                    }
                    else if(condition == PersonCondition.Normal)
                    {
                        SoulEnergySystem.Instance.Consume(0);
                        break;
                    }
                    else if(condition == PersonCondition.Tired)
                    {
                        SoulEnergySystem.Instance.Consume(1);
                        break;
                    }
                }
                break;

            case "SoundTrigger":
            default:
                if (!SoulEnergySystem.Instance.HasEnoughEnergy(3))
                {
                    UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다", 2f);
                    return false;
                }
                else
                {
                    SoulEnergySystem.Instance.Consume(3);
                }
                break;
        }

        if (obssessingTarget.tag == "HideArea")
        {
            UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode($"은신 시도 중...", 2f);
        }
        else
        {
            UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode($"빙의 시도 중...", 2f);
        }

        RequestObssession();
        return true;
    }

    public void RequestObssession()
    {
        switch (obssessingTarget.tag)
        {
            // 사람, 유인 오브젝트, 은신처만 QTE 요청
            case "SoundTrigger":
            case "HideArea":
                PossessionQTESystem.Instance.StartQTE();
                break;
            
            case "Person":
                PossessionQTESystem.Instance.StartQTE3();
                break;

            default:
                PossessionStateManager.Instance.StartPossessionTransition();
                break;
        }
    }

    // 빙의 가능 대상 설정
    public void SetInteractTarget(BasePossessable target)
    {
        //if (!target.HasActivated)
            //return;

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
        CanMove = false;
        Player.animator.SetTrigger("PossessIn");
    }

    public void StartPossessionOutSequence() // 빙의 해제 애니메이션
    {
        CanMove = false;
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