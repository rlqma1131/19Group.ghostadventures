using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static _01.Scripts.Utilities.Timer;

[RequireComponent(typeof(SpriteRenderer))]
public class StealthDoor : MonoBehaviour
{
    [Header("감지 설정")] 
    [SerializeField] float detectionRange = 3f;
    [SerializeField] float fadeSpeed = 2f;
    [SerializeField] float maxAlpha = 0.4f;
    [SerializeField] float updateInterval = 0.01f;

    [Header("추가 감지 대상 (Cat 등)")] 
    [SerializeField] List<GameObject> extraTargets;

    SpriteRenderer spriteRenderer;
    CountdownTimer updateTimer;
    float currentAlpha;
    float targetAlpha;

    readonly List<Transform> allTargets = new();
    readonly List<SpriteRenderer> allRenderers = new();

    void Awake() {
        if (allRenderers.Count <= 0) allRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));
    }

    void Start() {
        updateTimer = new CountdownTimer(updateInterval);
        updateTimer.OnTimerStop = () => {
            UpdateTargetAlpha();
            updateTimer.Start();
        };
        
        SetAlpha(0f);
        RegisterTargets();
        updateTimer.Start();
    }

    void RegisterTargets() {
        allTargets.Clear();

        // Player 자동 등록
        allTargets.Add(GameManager.Instance.Player.transform);

        // 인스펙터로 받은 추가 타겟 등록
        foreach (GameObject go in extraTargets.Where(go => go)) {
            allTargets.Add(go.transform);
        }
    }

    void Update() {
        UpdateAlpha();
        
        updateTimer.Tick(Time.deltaTime);
    }

    Transform FindNearestTarget() {
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Transform t in allTargets) {
            if (!t || !t.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, t.position);
            if (dist < closestDist) {
                closest = t;
                closestDist = dist;
            }
        }

        return closest;
    }

    void UpdateTargetAlpha() {
        Transform nearest = FindNearestTarget();

        if (nearest) {
            float dist = Vector2.Distance(transform.position, nearest.position);
            targetAlpha = dist <= detectionRange ? maxAlpha : 0f;
        }
        else targetAlpha = 0f;
    }

    void UpdateAlpha() {
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        SetAlpha(currentAlpha);
    }

    void SetAlpha(float alpha) {
        foreach (SpriteRenderer renderVal in allRenderers) {
            if (!renderVal) continue;

            Color color = renderVal.color;
            color.a = alpha;
            renderVal.color = color;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}