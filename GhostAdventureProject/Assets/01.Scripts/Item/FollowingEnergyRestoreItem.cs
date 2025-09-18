using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;

public class FollowingEnergyRestoreItem : BaseInteractable
{
    [SerializeField] private float offsetX = 1.5f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private int bonusRestoreAmount = 5;
    [SerializeField] private float restoreInterval = 1f;

    private bool isFollowing = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            TryActivate();
        }

        if (isFollowing && player)
        {
            Vector3 targetPos = player.transform.position + new Vector3(offsetX, 0, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }

    void TryActivate()
    {
        if (player.InteractSystem.CurrentClosest != this.gameObject) return;

        if (isFollowing) return;
        
        isFollowing = true;

        player.SoulEnergy.SetRestoreBoost(restoreInterval, player.SoulEnergy.baseRestoreAmount + bonusRestoreAmount);
        player.SoulEnergy.EnableHealingEffect();

        player.InteractSystem.RemoveInteractable(gameObject);
        if (Highlight != null) Highlight.SetActive(false);
        player.InteractSystem.GetEKey().SetActive(false);
    }

    public void DestroyItem() {
        if (isFollowing)
        {
            player.SoulEnergy.ResetRestoreBoost();
            player.SoulEnergy.DisableHealingEffect();
        }

        Destroy(gameObject);
    }

    // ✅ 여기 추가: 트리거 진입/이탈 처리
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")&& !isFollowing)
        {
            player.InteractSystem.AddInteractable(this.gameObject);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.InteractSystem.RemoveInteractable(this.gameObject);
            if (Highlight != null)
                Highlight.SetActive(false);
        }
    }
}