using UnityEngine;

public abstract class BasePossessable : MonoBehaviour
{
    [SerializeField] protected Animator anim;
    [SerializeField] protected bool hasActivated;
    public bool isPossessed;
    public bool HasActivated => hasActivated;
    public bool IsPossessed => isPossessed;
    public bool IsPossessedState => isPossessed;



    protected virtual void Start()
    {
        isPossessed = false;
        hasActivated = true;
        // 해금 안된 오브젝트는 hasActivated 값 false로 초기화 해주기
    }

    protected virtual void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            Unpossess();
    }

    // 상호작용 메시지 표시 대상 설정
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated)
            return;

        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    public void Unpossess()
    {
        Debug.Log("빙의 해제");
        isPossessed = false;
        PossessionStateManager.Instance.StartUnpossessTransition();
    }

    public virtual void OnQTESuccess()
    {
        Debug.Log("QTE 성공 - 빙의 완료");

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    public void OnQTEFailure()
    {
        Debug.Log("QTE 실패 - 빙의 취소");
        isPossessed = false;
        SoulEnergySystem.Instance.Consume(1);
    }
    public virtual void OnPossessionEnterComplete() { }
}
