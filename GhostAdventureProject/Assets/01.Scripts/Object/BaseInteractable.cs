using UnityEngine;

/// <summary>
/// 상호작용키 팝업 기능 구현하는 클래스
/// 어떤 오브젝트와 상호작용할지는 PlayerInteractSystem.cs 에서 관리
/// </summary>
public class BaseInteractable : MonoBehaviour
{
    public GameObject interactionInfo;

    void Start()
    {
        if (interactionInfo != null)
            interactionInfo.SetActive(false);
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
