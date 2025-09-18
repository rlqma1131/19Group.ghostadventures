using System;
using System.Collections.Generic;
using _01.Scripts.Extensions;
using _01.Scripts.Object.BaseClasses.Interfaces;
using _01.Scripts.Player;
using UnityEngine;
using static _01.Scripts.Utilities.Timer;

/// <summary>
/// 가까운 하나의 오브젝트만 상호작용가능하게(상호작용키 팝업되도록) 하는 클래스
/// 상호작용 팝업은 BaseInteractable.cs
/// </summary>
public class PlayerInteractSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public GameObject eKey;
    [SerializeField] GameObject currentClosest;

    [Header("Sensor Settings")] 
    [SerializeField] LayerMask detectableLayer;
    [SerializeField] float radius = 0.5f;
    [SerializeField] float checkInterval = 0.1f;
    [SerializeField] int checkCountThreshold = 12;
    [SerializeField] float checkDistanceThreshold = 0.001f;

    CountdownTimer checkTimer;
    Collider2D[] results;
    
    public GameObject CurrentClosest => currentClosest;// 디버깅용

    HashSet<GameObject> nearbyInteractables = new();

    public void Initialize() {
        results = new Collider2D[checkCountThreshold];
        
        checkTimer = new CountdownTimer(checkInterval);
        checkTimer.OnTimerStop += () => {
            UpdateClosest();
            checkTimer.Start();
        };
        
        checkTimer.Start();
    }

    void Update() => checkTimer.Tick(Time.unscaledDeltaTime);
    
    GameObject GetClosestGameObject() {
        GameObject newClosest = null;
        float distance = float.MaxValue;
        
        int size = Physics2D.OverlapCircleNonAlloc(transform.position, radius, results, detectableLayer);
        for (int i = 0; i < size; i++) {
            if (!results[i]) continue;
            
            float newDist = Vector2.Distance(transform.position, results[i].transform.position);
            if (!newClosest || newDist < distance + checkDistanceThreshold &&
                nearbyInteractables.Contains(results[i].gameObject) &&
                newClosest.CompareLayerPriority(results[i].gameObject) <= 0) {
                if (!results[i].gameObject.TryGetComponent(out IPossessable possessable) || possessable.HasActivated())
                    newClosest = results[i].gameObject;
                else if (results[i].gameObject.TryGetComponent(out MemoryFragment fragment))
                    if (fragment.IsScannable) newClosest = results[i].gameObject;
            }
            
            distance = newDist;
        }
        
        return newClosest;
    }

    void UpdateClosest() {
        GameObject newClosest = GetClosestGameObject();

        if (currentClosest == newClosest) return;
        
        // 이전 오브젝트 처리
        if (currentClosest) {
            // 팝업 끄기
            if (currentClosest.TryGetComponent(out IInteractable prevInteractable)) {
                eKey.SetActive(false);
                prevInteractable.ShowHighlight(false);
            }
        }

        currentClosest = newClosest;

        // 새 오브젝트 처리
        if (!currentClosest) return;
        
        if (currentClosest.TryGetComponent(out MemoryFragment memory)) {
            if (!memory.IsScannable) {
                eKey.SetActive(false);
                memory.ShowHighlight(false);
                return;
            }
        }

        // 팝업 켜기
        // 상호작용 가능 물체가 아닐 시 Highlight Object 띄우지 않기
        if (!currentClosest.TryGetComponent(out IInteractable nextInteractable)) return;
        
        // 만약 Possessable Object라면 현재 빙의 가능 상태면 Highlight Object 작동
        if (currentClosest.TryGetComponent(out IPossessable nextPossessable)) {
            if (!nextPossessable.HasActivated()) return;
            eKey.SetActive(true);
            nextInteractable.ShowHighlight(true);
        }
        else {
            eKey.SetActive(true);
            nextInteractable.ShowHighlight(true);
        }
    }

    // 플레이어 근처에 있는 오브젝트들
    public void AddInteractable(GameObject obj) {
        if (!obj) return;
        nearbyInteractables.Add(obj);
    }

    // 플레이어 근처를 벗어난 오브젝트들
    public void RemoveInteractable(GameObject obj) {
        if (!nearbyInteractables.Contains(obj)) return;
        nearbyInteractables.Remove(obj);
    }

    public GameObject GetEKey() => eKey;
}
