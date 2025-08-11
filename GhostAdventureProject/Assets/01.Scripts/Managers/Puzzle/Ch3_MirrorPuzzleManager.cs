using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ch3_MirrorPuzzleManager : MonoBehaviour
{
    public static Ch3_MirrorPuzzleManager Instance { get; private set; }

    private int roomsCleared = 0;
    public int totalRooms = 3;
    private GameObject player;

    [SerializeField] private GameObject hintPrefab;
    [SerializeField] private Ch3_MemoryPositive_Mirror memory;
    
    private bool firstRoomEntered = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    public void NotifyFirstRoomEntered()
    {
        if (!firstRoomEntered)
        {
            firstRoomEntered = true;
            UIManager.Instance.PromptUI.ShowPrompt("이 방은 뭐지..? 이런 방이 더 있을까?", 2f);
        }
    }
    
    public void NotifyRoomCleared()
    {
        roomsCleared++;
        
        if (roomsCleared == 1 || roomsCleared == 2)
        {
            string[] prompts =
            {
                "거울 공간이 사라졌어..이런 공간이 더 있을까?",
                "이런 병실을 더 찾아보자"
            };
            string randomPrompt = prompts[UnityEngine.Random.Range(0, prompts.Length)];
            UIManager.Instance.PromptUI.ShowPrompt(randomPrompt, 2f);
        }
        
        if (roomsCleared >= totalRooms)
        {
            memory.ActivateObj();
            UIManager.Instance.PromptUI.ShowPrompt("이게 마지막인가..? 저기 뭔가 떨어져있어!", 2f);
            ShowHintNearPlayer();
        }
    }

    private void ShowHintNearPlayer()
    {
        if (player == null) return;

        Vector3 targetPos = player.transform.position + player.transform.right * 1.5f + Vector3.up * 0.5f;
        Vector3 spawnPos = targetPos + Vector3.up * 2f;
        
        GameObject hint = Instantiate(hintPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer sr = hint.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
            Sequence seq = DOTween.Sequence();

            seq.Append(sr.DOFade(1f, 1f));
            seq.Join(hint.transform.DOMove(targetPos, 1f).SetEase(Ease.OutCubic));
            
            seq.OnComplete(() =>
            {
                PossessionSystem.Instance.CanMove = true; 
            });
        }
        
        PossessionSystem.Instance.CanMove = false; // DOTween 도중에는 멈춤
    }
}
