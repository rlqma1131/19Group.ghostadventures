using _01.Scripts.Player;
using UnityEngine;

public abstract class BaseUnlockObject : MonoBehaviour
{
    protected Animator anim;
    protected Player player;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        player = GameManager.Instance.Player;
    }

    public abstract void Unlock();

    // 상호작용키 UI 표시
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        player.InteractSystem.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        player.InteractSystem.RemoveInteractable(gameObject);
    }
}
