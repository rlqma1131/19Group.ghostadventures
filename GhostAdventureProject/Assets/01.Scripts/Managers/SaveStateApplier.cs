using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SaveStateApplier : Singleton<SaveStateApplier>
{
    private void OnEnable()
    {
        SaveManager.Loaded += OnSaveLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SaveManager.CurrentData != null && gameObject.activeInHierarchy)
            StartCoroutine(ApplyNextFrame());
    }
    private void OnDisable()
    {
        SaveManager.Loaded -= OnSaveLoaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSaveLoaded(SaveData _)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ApplyNextFrame()); //다음 프레임에 적용 (Start 이후에 적용을 위해)
        else
            ApplySavedStatesInScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SaveManager.CurrentData != null)
            StartCoroutine(ApplyNextFrame()); //다음 프레임에 적용
    }

    private IEnumerator ApplyNextFrame()
    {
        yield return null; // 한 프레임 대기 -> 모든 Start() 이후
        ApplySavedStatesInScene();
    }

    private void ApplySavedStatesInScene()
    {
        var data = SaveManager.CurrentData;
        if (data == null) return;

        // BasePossessable
        foreach (var p in FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetObjectPosition(uid.Id, out var pos))
            {
                // Rigidbody2D 있으면 물리 좌표로 세팅
                if (p.TryGetComponent<Rigidbody2D>(out var rb))
                    rb.position = pos;
                else
                    p.transform.position = pos;
            }
        }

        // MemoryFragment
        foreach (var m in FindObjectsOfType<MemoryFragment>(true))
        {
            if (!m.TryGetComponent(out UniqueId uid)) continue;

            // 1) 저장에 명시적으로 있으면 그 값 사용
            if (SaveManager.TryGetMemoryFragmentScannable(uid.Id, out bool scannable))
            {
                m.ApplyFromSave(scannable);
                continue;
            }
            // 2) 저장에 없더라도, 이미 수집된 MemoryData면 스캔 불가로 강제
            //    (collectedMemoryIDs에 있으면 이미 스캔됨)
            if (SaveManager.HasCollectedMemoryID(m.data.memoryID))
            {
                m.ApplyFromSave(false);
            }
            // 3) 그 외엔 인스펙터 기본값 유지
        }
        // MemoryFragment 위치도 셋팅
        foreach (var m in FindObjectsOfType<MemoryFragment>(true))
        {
            if (m.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetObjectPosition(uid.Id, out var pos))
            {
                if (m.TryGetComponent<Rigidbody2D>(out var rb))
                    rb.position = pos;
                else
                    m.transform.position = pos;
            }
        }

        // 문
        foreach (var door in FindObjectsOfType<BaseDoor>(true))
        {
            if (door.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetDoorLocked(uid.Id, out bool locked))
            {
                door.SetLockedFromSave(locked);
            }
        }

        // BasePossessable 활성/비활성 적용
        foreach (var p in FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetActiveState(uid.Id, out bool activeGO))
            {
                p.gameObject.SetActive(activeGO);
            }
        }

        // MemoryFragment 활성/비활성 적용
        foreach (var m in FindObjectsOfType<MemoryFragment>(true))
        {
            if (m.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetActiveState(uid.Id, out bool activeGO))
            {
                m.gameObject.SetActive(activeGO);
            }
        }

        // 플레이어 인벤토리 적용
        var inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        SaveManager.ApplyPlayerInventoryFromSave(inv);

        // 빙의 대상 인벤토리 적용
        foreach (var have in GameObject.FindObjectsOfType<HaveItem>(true))
        {
            if (!have.TryGetComponent(out UniqueId uid)) continue;
            if (!SaveManager.TryGetPossessableInventory(uid.Id, out var entries)) continue;

            // 1) 기존 슬롯 정리
            have.inventorySlots.RemoveAll(s => s == null || s.item == null || s.quantity <= 0);

            // 2) 엔트리 기준으로 재구성
            var newList = new List<InventorySlot_PossessableObject>();
            foreach (var e in entries)
            {
                // ItemData 로드(에셋 파일명 기준)
                var item = Resources.Load<ItemData>("ItemData/" + e.itemKey);
                if (item == null) { Debug.LogWarning($"[Restore] ItemData not found: {e.itemKey}"); continue; }

                // 슬롯 컴포넌트가 반드시 필요하면 간단히 생성해서 보관용으로 씀
                var slotGo = new GameObject($"Slot_{e.itemKey}");
                slotGo.transform.SetParent(have.transform, false);
                var slot = slotGo.AddComponent<InventorySlot_PossessableObject>();
                slot.item = item;
                slot.quantity = e.quantity;
                newList.Add(slot);
            }

            have.inventorySlots = newList;
            // 필요 시: 현재 빙의 UI가 열려있으면 갱신 트리거 (보통 OpenInventory 때 새로 뿌려서 불필요)
        }
    }
}
