using _01.Scripts.Extensions;
using _01.Scripts.Object.BaseClasses.Interfaces;
using _01.Scripts.Player;
using UnityEngine;

public abstract class BasePossessable : MonoBehaviour, IInteractable, IPossessable
{
    [Header("Base Interactable References")]
    [SerializeField] protected GameObject highlightObj;

    [Header("Base Possessable Reference")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected AudioClip possessionSFX;
    
    [Header("Current State of Possessable Object")]
    [SerializeField] protected bool hasActivated; // 빙의가 가능한 상태인지 여부
    [SerializeField] protected bool isPossessed;
    
    // Fields
    protected Player player;
    
    // Properties
    public GameObject Highlight => highlightObj;
    public bool IsPossessed => isPossessed;

    /// <summary>
    /// Highlight Object의 컴포넌트를 검출하여 그것의 게임오브젝트를 캐쉬
    /// 오버라이드 필요 시 base.Awake() 사용 필수
    /// </summary>
    protected virtual void Awake() {
        Transform component = 
            gameObject.GetComponentInChildren_SearchByName<Transform>("Highlight", true);
        highlightObj = component != null ? component.gameObject : null;
    }
    
    /// <summary>
    /// 파라미터 Initialize
    /// </summary>
    protected virtual void Start() {
        player = GameManager.Instance.Player;
        isPossessed = false;
        hasActivated = true;
        
        highlightObj?.SetActive(false);
    }

    /// <summary>
    /// 매 프레임 마다 갱신되어야 할 행동들
    /// base.Update는 선택(Optional)
    /// </summary>
    protected virtual void Update() {
        if (Input.GetKeyDown(KeyCode.E) && isPossessed) Unpossess();
        TriggerEvent();
    }
    
    /// <summary>
    /// Show or Hide Highlight of object
    /// </summary>
    /// <param name="pop"></param>
    public void ShowHighlight(bool pop) => highlightObj?.SetActive(pop);

    public bool HasActivated() => hasActivated;
    
    public void SetActivated(bool value) => hasActivated = value;

    /// <summary>
    /// Events will activate in every frame
    /// </summary>
    public virtual void TriggerEvent() { }

    /// <summary>
    /// Called when the user tries to possess the object
    /// </summary>
    /// <returns></returns>
    public bool TryPossess() {
        switch (tag) {
            case "Scanner":
            case "Cat": break;
            case "Person":
                // 사람 구현되면 피로도에 따라 소모량 조정
                if (!player.SoulEnergy.HasEnoughEnergy(1)) {
                    UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다");
                    return false;
                }

                PersonCondition condition = GetComponent<PersonConditionUI>().currentCondition;
                switch (condition) {
                    case PersonCondition.Vital: player.SoulEnergy.Consume(-1); break;
                    case PersonCondition.Normal: player.SoulEnergy.Consume(0); break;
                    case PersonCondition.Tired: player.SoulEnergy.Consume(1); break;
                    default: player.SoulEnergy.Consume(0); break;
                }
                break;
            default:
                if (!player.SoulEnergy.HasEnoughEnergy(1)) {
                    UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다");
                    return false;
                }

                player.SoulEnergy.Consume(1);
                break;
        }

        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode(CompareTag("HideArea") ? "은신 시도 중..." : "빙의 시도 중...", 2f);
        player.PossessionSystem.PossessedTarget = this;
        RequestQTEEvent();
        return true;
    }

    /// <summary>
    /// Called when user tries to be unpossessed from the object
    /// </summary>
    public virtual void Unpossess() {
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("빙의 해제", 2f);
        isPossessed = false;
        PossessionStateManager.Instance.StartUnpossessTransition();
        player.PossessionSystem.PossessedTarget = null;
    }
    
    public virtual void OnPossessionEnterComplete() => isPossessed = true;

    public virtual void OnPossessionEnterFailed() { }

    #region QTE Event Handling Methods
    /// <summary>
    /// Request QTE Event when the user starts to possess the object
    /// </summary>
    void RequestQTEEvent() {
        switch (tag) {
            // 사람, 은신처만 QTE 요청
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
    
    /// <summary>
    /// Called when the user successfully cleared QTE Event
    /// </summary>
    public virtual void OnQTESuccess() => PossessionStateManager.Instance.StartPossessionTransition();

    /// <summary>
    /// Called when the user failed to clear QTE Event
    /// </summary>
    public void OnQTEFailure() => isPossessed = false;
    #endregion

    /// <summary>
    /// Register Object in nearby object cache located in player class
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (!hasActivated) return;

        if (other.CompareTag("Player")) {
            player.InteractSystem.AddInteractable(gameObject);
        }
    }
    
    /// <summary>
    /// Unregister object in nearby object cache located in player class
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            ShowHighlight(false);
            player.InteractSystem.RemoveInteractable(gameObject);
        }
    }

    #region Object State Refresh Methods
    // 상태 기록
    protected void MarkActivatedChanged() {
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetPossessableObjectState(uid.Id, gameObject.activeInHierarchy, transform.position, hasActivated);
    }
    #endregion
}
