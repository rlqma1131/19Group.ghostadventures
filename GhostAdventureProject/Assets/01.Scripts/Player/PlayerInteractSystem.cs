using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 가까운 하나의 오브젝트만 상호작용가능하게(상호작용키 팝업되도록) 하는 클래스
/// 상호작용 팝업은 BaseInteractable.cs
/// </summary>
public class PlayerInteractSystem : Singleton<PlayerInteractSystem>
{
    private List<GameObject> nearbyInteractables = new();
    [SerializeField] private GameObject currentClosest; 
    public GameObject CurrentClosest => currentClosest;// 디버깅용

    private void Update()
    {
        if (nearbyInteractables.Count == 0)
        {
            UpdateClosest(null);
            return;
        }

        GameObject closest = null;
        float closestDist = float.MaxValue;

        // 가장 가까운 오브젝트 판별
        foreach (var obj in nearbyInteractables)
        {
            if (obj == null) continue;
            float dist = Vector3.Distance(GameManager.Instance.Player.transform.position, obj.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = obj;
            }
        }
        UpdateClosest(closest);
    }

    private void UpdateClosest(GameObject newClosest)
    {
        // 이전 오브젝트 처리
        if (currentClosest != null && currentClosest != newClosest)
        {
            // 팝업 끄기
            if (currentClosest.TryGetComponent<BaseInteractable>(out var prevInteractable))
            {
                prevInteractable.SetInteractionPopup(false);
            }
        }

        currentClosest = newClosest;

        // 새 오브젝트 처리
        if (currentClosest != null)
        {
            // 팝업 켜기
            if (currentClosest.TryGetComponent<BaseInteractable>(out var nextInteractable))
            {
                nextInteractable.SetInteractionPopup(true);
            }
        }
    }

    // 플레이어 근처에 있는 오브젝트들
    public void AddInteractable(GameObject obj)
    {
        if (obj == null)
            return;

        nearbyInteractables.Add(obj);
    }

    // 플레이어 근처를 벗어난 오브젝트들
    public void RemoveInteractable(GameObject obj)
    {
        if (nearbyInteractables.Contains(obj))
        {
            nearbyInteractables.Remove(obj);
            if (currentClosest == obj)
            {
                UpdateClosest(null);
            }
        }
    }
}
