using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingEnergyRestoreItem : BaseInteractable
{
    [SerializeField] private float offsetX = 1.5f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private int bonusRestoreAmount = 5;
    [SerializeField] private float restoreInterval = 1f;

    private Transform player;
    private bool isFollowing = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryActivate();
        }

        if (isFollowing && player != null)
        {
            Vector3 targetPos = player.position + new Vector3(offsetX, 0, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }

    void TryActivate()
    {
        if (PlayerInteractSystem.Instance.CurrentClosest != this.gameObject)
            return;

        if (isFollowing) return;

        player = GameManager.Instance.Player.transform;
        isFollowing = true;

        SoulEnergySystem.Instance.SetRestoreBoost(restoreInterval, SoulEnergySystem.Instance.baseRestoreAmount + bonusRestoreAmount);
        SoulEnergySystem.Instance.EnableHealingEffect();

        PlayerInteractSystem.Instance.RemoveInteractable(this.gameObject);
        if (Highlight != null) Highlight.SetActive(false);
        PlayerInteractSystem.Instance.GetEKey().SetActive(false);
    }

    public void DestroyItem()
    {
        if (isFollowing)
        {
            SoulEnergySystem.Instance.ResetRestoreBoost();
            SoulEnergySystem.Instance.DisableHealingEffect();
        }

        Destroy(gameObject);
    }

    // ✅ 여기 추가: 트리거 진입/이탈 처리
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")&& !isFollowing)
        {
            PlayerInteractSystem.Instance.AddInteractable(this.gameObject);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInteractSystem.Instance.RemoveInteractable(this.gameObject);
            if (Highlight != null)
                Highlight.SetActive(false);
        }
    }
}