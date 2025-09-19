using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;

public class PossessionStateManager : Singleton<PossessionStateManager>
{
    public enum State { Ghost, Possessing }
    public State currentState { get; private set; } = State.Ghost;

    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    Player player;
    private Transform PlayerTransform
        => player.transform;
    private GameObject Player
        => player.gameObject;
    private BasePossessable possessedTarget;

    public bool IsPossessing() => currentState == State.Possessing;

    public void Initialize_Player(Player player) {
        this.player = player;
    }

    public void StartPossessionTransition() // 빙의 전환 실행 ( 빙의 애니메이션도 함께 )
    {
        SoulEnergySystem.Instance?.ResetRestoreBoost();
        SoulEnergySystem.Instance?.DisableHealingEffect();

        possessedTarget = player.PossessionSystem.CurrentTarget;
        player.PossessionSystem.PlayPossessionInAnimation();
    }

    public void PossessionInAnimationComplete() // 빙의 애니메이션 종료 후 빙의 전환 완료 처리
    {
        Player.SetActive(false);
        player.PossessionSystem.CanMove = true;
        
        // 추가적인 연출이나 효과
        // 빙의오브젝트 강조효과, 사운드 등
        
        currentState = State.Possessing;
        Debug.Log("target" + possessedTarget);
        UIManager.Instance.unpossessKey.SetActive(true);
        if(possessedTarget.CompareTag("Person")) UIManager.Instance.tabkeyUI.SetActive(true);
        UIManager.Instance.Inventory_PossessableObjectUI.OpenInventory(possessedTarget); // 빙의 인벤토리 표시됨
    }

    public void StartUnpossessTransition() // 빙의 해체 요청 ( 위치 이동 , 활성화, 빙의 해제 애니메이션 실행 )
    {
        PlayerTransform.position = possessedTarget.transform.position + spawnOffset;
        Player.SetActive(true);
        player.PossessionSystem.PlayPossessionOutSequence();
        UIManager.Instance.unpossessKey.SetActive(false);
        if(possessedTarget.CompareTag("Person")) UIManager.Instance.tabkeyUI.SetActive(false);
        UIManager.Instance.Inventory_PossessableObjectUI.HideInventory(); // 빙의 인벤토리 사라짐
    }
    
    public void PossessionOutAnimationComplete() // 빙의 해제 애니메이션 종료 후 상태 복귀
    {
        player.PossessionSystem.CanMove = true;
        currentState = State.Ghost;
    }
}
