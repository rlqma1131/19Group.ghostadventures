using System.Collections;
using UnityEngine;

public class PossessionSystem : MonoBehaviour
{
    // 싱글톤
    public static PossessionSystem Instance { get; private set; }

    [Header("SFX")] 
    [SerializeField] AudioClip possessIn;
    [SerializeField] AudioClip possessOut;
    [SerializeField] GameObject scanPanel;
    [SerializeField] BasePossessable currentTarget; // 디버깅용

    BasePossessable obsessingTarget;
    PlayerController player;
    public BasePossessable CurrentTarget => currentTarget;

    public bool CanMove { get; set; } = true;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    void Start() {
        player = GameManager.Instance.PlayerController;
    }

    void OnTriggerEnter2D(Collider2D other) {
        //Debug.Log($"트리거 충돌: {other.name}");
        if (!other.TryGetComponent(out BasePossessable possessionObject)) return;

        SetInteractTarget(possessionObject);
    }

    void OnTriggerExit2D(Collider2D other) {
        if (!other.TryGetComponent(out BasePossessable possessionObject)) return;

        ClearInteractionTarget(possessionObject);
    }

    public bool TryPossess() {
        obsessingTarget = currentTarget;

        switch (obsessingTarget.tag) {
            case "Scanner":
            case "Cat": break;
            case "Person":
                // 사람 구현되면 피로도에 따라 소모량 조정
                if (!SoulEnergySystem.Instance.HasEnoughEnergy(1)) {
                    UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다");
                    return false;
                }

                PersonCondition condition = obsessingTarget.GetComponent<PersonConditionUI>().currentCondition;
                switch (condition) {
                    case PersonCondition.Vital: SoulEnergySystem.Instance.Consume(-1); break;
                    case PersonCondition.Normal: SoulEnergySystem.Instance.Consume(0); break;
                    case PersonCondition.Tired: SoulEnergySystem.Instance.Consume(1); break;
                    default: SoulEnergySystem.Instance.Consume(0); break;
                }

                break;
            default:
                if (!SoulEnergySystem.Instance.HasEnoughEnergy(1)) {
                    UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다");
                    return false;
                }

                SoulEnergySystem.Instance.Consume(1);
                break;
        }

        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode(
            obsessingTarget.CompareTag("HideArea") ? "은신 시도 중..." : "빙의 시도 중...", 2f);
        RequestObsession();
        return true;
    }

    public void RequestObsession() {
        switch (obsessingTarget.tag) {
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
    public void SetInteractTarget(BasePossessable target) {
        //if (!target.HasActivated)
        //return;

        currentTarget = target;
        if (player != null)
            player.currentTarget = currentTarget;
    }

    public void ClearInteractionTarget(BasePossessable target) {
        if (currentTarget == target) {
            currentTarget = null;
            if (player != null)
                player.currentTarget = null;
        }
    }

    public void PlayPossessionInAnimation() // 빙의 시작 애니메이션
    {
        CanMove = false;
        player.Animator.SetTrigger("PossessIn");
        SoundManager.Instance.PlaySFX(possessIn);

        EnemyAI.PauseAllEnemies();
    }

    public void PlayPossessionOutSequence() // 빙의 해제 애니메이션
    {
        CanMove = false;
        StartCoroutine(DelayedPossessionOutPlay());
        SoundManager.Instance.PlaySFX(possessOut);

        EnemyAI.PauseAllEnemies();
    }

    IEnumerator DelayedPossessionOutPlay() {
        yield return null; // 한 프레임 딜레이
        player.Animator.Play("Player_PossessionOut");
    }
    
    #region Animation Events
    // Animation Events => Do not change name of methods
    public void OnPossessionInAnimationComplete() // 빙의 시작 애니메이션 후 이벤트
    {
        EnemyAI.ResumeAllEnemies();
        PossessionStateManager.Instance.PossessionInAnimationComplete();

        if (obsessingTarget != null) {
            // ex) 애니메이션 끝나고 확대
            obsessingTarget.isPossessed = true;
            obsessingTarget.OnPossessionEnterComplete();
        }
        else {
            Debug.LogWarning("빙의 대상이 설정되지 않아서 이벤트가 호출되지 않았어요!");
        }
    }

    public void OnPossessionOutAnimationComplete() // 빙의 해제 애니메이션 후 이벤트
    {
        PossessionStateManager.Instance.PossessionOutAnimationComplete();
        EnemyAI.ResumeAllEnemies();
    }
    #endregion
}