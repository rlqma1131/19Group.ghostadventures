using UnityEngine;

// 하이라이터 인터페이스
public interface IHighlightable
{
    void SetHighlight(bool isActive);
}

/// <summary>
/// 상호작용키 팝업 기능 구현하는 클래스
/// 어떤 오브젝트와 상호작용할지는 PlayerInteractSystem.cs 에서 관리
/// </summary>
public class BaseInteractable : MonoBehaviour, IHighlightable
{
    public GameObject interactionInfo;

    [Header("Outline Materials")]
    [SerializeField] protected Material defaultOutline;
    [SerializeField] protected Material memoryOutline;
    [SerializeField] protected Material obssessionOutline;

    protected Material originalMaterial;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.material;
    }

    void Start()
    {
        if (interactionInfo != null)
            interactionInfo.SetActive(false);
    }

    // 상호작용 할 오브젝트 하이라이팅
    public virtual void SetHighlight(bool isActive)
    {
        if (spriteRenderer == null) return;

        if (isActive)
            spriteRenderer.material = GetHighlightMaterial();
        else
            spriteRenderer.material = originalMaterial;
    }
    
    // 자식 클래스에서 타입에 따라 다른 아웃라인 머티리얼 반환
    protected virtual Material GetHighlightMaterial()
    {
        return defaultOutline;
    }

    public void SetInteractionPopup(bool pop)
    {
        if (interactionInfo != null)
        {
            interactionInfo.SetActive(pop);
        }
    }

    // 은신처일때만 적용 (외에는 각 스크립트에서 관리)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.CompareTag("HideArea")) 
        {
            if (collision.CompareTag("Player"))
            {
                SetInteractionPopup(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetInteractionPopup(false);
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
