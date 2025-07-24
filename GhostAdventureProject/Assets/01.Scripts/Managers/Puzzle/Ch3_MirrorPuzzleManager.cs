using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MirrorPuzzleManager : MonoBehaviour
{
    public static Ch3_MirrorPuzzleManager Instance;

    private int roomsCleared = 0;
    public int totalRooms = 3;
    private GameObject player;

    [SerializeField] private GameObject hintPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    public void NotifyRoomCleared(GameObject mirrorRoom)
    {
        roomsCleared++;
        if (roomsCleared >= totalRooms)
        {
            ShowHintNearPlayer();
        }
    }

    private void ShowHintNearPlayer()
    {
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + player.transform.right * 1.5f + Vector3.up * 0.5f;
        Instantiate(hintPrefab, spawnPos, Quaternion.identity);
    }
}
