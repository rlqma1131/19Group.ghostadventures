using UnityEngine;
using DG.Tweening;

public class Ch2_Lazer : MonoBehaviour
{
    [SerializeField] private float knockbackDistance = 1.5f;
    [SerializeField] private float knockbackDuration = 0.5f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            SoulEnergySystem.Instance.Consume(1);
            PossessionSystem.Instance.CanMove = false;

            Transform playerTr = GameManager.Instance.Player.transform;
            Vector3 knockbackTarget = playerTr.position + Vector3.left * knockbackDistance;

            Debug.Log("[Ch2_Lazer] 플레이어와 트리거 충돌 - 넉백 Tween");

            playerTr.DOMove(knockbackTarget, knockbackDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    Invoke(nameof(OnKnockbackEnd), 0.8f);
                });
        }
    }

    private void OnKnockbackEnd()
    {
        PossessionSystem.Instance.CanMove = true;
    }
}

