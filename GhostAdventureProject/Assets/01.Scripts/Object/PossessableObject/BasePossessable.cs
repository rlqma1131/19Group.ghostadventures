using UnityEngine;

public abstract class BasePossessable : BaseInteractable
{
    [Header("Base Possessable Reference")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected AudioClip possessionSFX;
    
    [Header("Current State of Possessable Object")]
    [SerializeField] protected bool hasActivated; // 빙의가 가능한 상태인지 여부
    [SerializeField] public bool isPossessed;
    
    public bool HasActivated => hasActivated;
    public bool IsPossessedState => isPossessed;

    new protected virtual void Start() {
        highlightObj?.SetActive(false);
        isPossessed = false;
        hasActivated = true;
    }

    protected virtual void Update() {
        if (Input.GetKeyDown(KeyCode.E) && isPossessed) Unpossess();
        TriggerEvent();
    }
    
    // 상호작용 메시지 표시 대상 설정
    protected override void OnTriggerEnter2D(Collider2D other) {
        if (!hasActivated) return;

        base.OnTriggerEnter2D(other);
    }

    public virtual void Unpossess()
    {
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("빙의 해제", 2f);
        isPossessed = false;
        PossessionStateManager.Instance.StartUnpossessTransition();
    }

    public virtual void OnQTESuccess()
    {
        // 빙의 효과음
        //SoundManager.Instance.PlaySFX(possessionSFX);

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    public void OnQTEFailure()
    {
        isPossessed = false;
    }

    // 빙의 애니메이션이 끝나면 호출되는 메서드
    // 필요에 따라 빙의애니메이션 끝나면 구현되는 기능들을 넣어주세요.
    public virtual void OnPossessionEnterComplete() { }

    public virtual void CantPossess() { }

    // 로드 시 상태 셋업
    public void ApplyHasActivatedFromSave(bool value)
    {
        if (hasActivated == value) return;
        hasActivated = value;
        OnRestoredHasActivated(value);
    }

    // 추후 VFX/콜라이더/애니 갱신 등
    protected virtual void OnRestoredHasActivated(bool value) { }

    // 상태 기록
    protected void MarkActivatedChanged()
    {
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetPossessableState(uid.Id, hasActivated);
    }
}
