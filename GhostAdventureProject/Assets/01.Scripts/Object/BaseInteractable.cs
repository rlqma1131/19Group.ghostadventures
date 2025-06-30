using UnityEngine;

public class BaseInteractable : MonoBehaviour
{
    public GameObject interactionInfo;
    private BasePossessable BasePossessable;
    private bool isCompleted;

    void Start()
    {
        if (interactionInfo != null)
            interactionInfo.SetActive(false);

        BasePossessable = GetComponent<BasePossessable>();

        if (BasePossessable != null)
            isCompleted = BasePossessable.isCompleted;
    }

    public void SetInteractionPopup(bool pop)
    {
        if (interactionInfo != null && isCompleted)
            interactionInfo.SetActive(pop);
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
