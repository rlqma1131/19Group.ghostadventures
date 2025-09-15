using System;
using _01.Scripts.Player;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class Ch2_Laser : MonoBehaviour
{
    [SerializeField] private float knockbackDistance = 1.5f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private PlayableDirector timelineDirector;

    Player player;
    
    void Start() {
        player = GameManager.Instance.Player;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            timelineDirector.Play();
            SoulEnergySystem.Instance.Consume(1);
            player.PossessionSystem.CanMove = false;
            GameManager.Instance.PlayerController.Animator.SetBool("Move", false);
            GameManager.Instance.PlayerController.Animator.Play("Hit");

            Transform playerTr = GameManager.Instance.PlayerObj.transform;
            Vector3 knockbackTarget = playerTr.position + Vector3.left * knockbackDistance;

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
        player.PossessionSystem.CanMove = true;
    }
}

