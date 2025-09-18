using System.Collections;
using _01.Scripts.Player;
using UnityEngine;

public class PossessionSystem : MonoBehaviour
{
    readonly static int PossessIn = Animator.StringToHash("PossessIn");

    [Header("SFX")] 
    [SerializeField] AudioClip possessIn;
    [SerializeField] AudioClip possessOut;
    [SerializeField] GameObject scanPanel;
    [SerializeField] BasePossessable currentTarget; // 디버깅용

    Player player;
    
    public BasePossessable PossessedTarget { get; set; }

    public bool CanMove { get; set; } = true;

    public void Initialize(Player player) {
        this.player = player;
    }
    
    public void TryPossess() {
        if (!CurrentTargetIsPossessable()) {
            currentTarget?.OnPossessionEnterFailed();
            return;
        }

        currentTarget.TryPossess();
    }
    
    bool CurrentTargetIsPossessable() {
        // 가까운 대상이 빙의 가능 상태인지 확인
        return currentTarget
               && player.InteractSystem.CurrentClosest == currentTarget.gameObject
               && !currentTarget.IsPossessed
               && currentTarget.HasActivated;
    }
    
    public void PlayPossessionInAnimation() // 빙의 시작 애니메이션
    {
        CanMove = false;
        player.Animator.SetTrigger(PossessIn);
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
    
    // Animation Events => Do not change the name of methods
    #region Animation Events
    public void OnPossessionInAnimationComplete() // 빙의 시작 애니메이션 후 이벤트
    {
        EnemyAI.ResumeAllEnemies();
        PossessionStateManager.Instance.PossessionInAnimationComplete();

        if (PossessedTarget) {
            // ex) 애니메이션 끝나고 확대
            Debug.Log($"Possessed Target : {PossessedTarget.name}");
            PossessedTarget.OnPossessionEnterComplete();
        }
        else {
            Debug.LogWarning("빙의 대상이 설정되지 않아서 이벤트가 호출되지 않았어요!");
        }
    }

    public void OnPossessionOutAnimationComplete() // 빙의 해제 애니메이션 후 이벤트
    {
        PossessionStateManager.Instance.PossessionOutAnimationComplete();
        EnemyAI.ResumeAllEnemies();
        PossessedTarget = null;
    }
    #endregion
    
    void OnTriggerEnter2D(Collider2D other) {
        //Debug.Log($"트리거 충돌: {other.name}");
        if (!other.TryGetComponent(out BasePossessable possessionObject)) return;

        SetInteractTarget(possessionObject);
    }

    void OnTriggerExit2D(Collider2D other) {
        if (!other.TryGetComponent(out BasePossessable possessionObject)) return;

        ClearInteractionTarget(possessionObject);
    }

    void SetInteractTarget(BasePossessable target) {
        currentTarget = target;
    }

    void ClearInteractionTarget(BasePossessable target) {
        if (currentTarget != target) return;
        currentTarget = null;
    }
}