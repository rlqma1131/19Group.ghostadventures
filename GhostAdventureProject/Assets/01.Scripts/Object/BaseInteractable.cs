using UnityEngine;

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

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetInteractionPopup(false);
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
