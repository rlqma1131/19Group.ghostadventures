using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractSystem : Singleton<PlayerInteractSystem>
{
    private List<GameObject> nearbyInteractables = new();
    private GameObject currentClosest;
    public GameObject CurrentClosest => currentClosest;

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
        if (currentClosest != null && currentClosest != newClosest)
        {
            var prev = currentClosest.GetComponent<BaseInteractable>();
            if (prev != null) prev.SetInteractionPopup(false);
        }

        currentClosest = newClosest;

        // 상호작용 가능키 표시
        if (currentClosest != null)
        {
            var next = currentClosest.GetComponent<BaseInteractable>();
            if (next != null)
                next.SetInteractionPopup(true);
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
