using System.Collections;
using System.Collections.Generic;
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
            StartCoroutine(ApplyNextFrame()); // Start 이후에 적용하기 위해 다음 프레임에
        else
            ApplySavedStatesInScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return; // 컷씬이면 적용 안함
        if (SaveManager.CurrentData != null)
        {
            StartCoroutine(ApplyNextFrame()); // 다음 프레임에 적용
        }
    }

    private IEnumerator ApplyNextFrame()
    {
        // 한 프레임 이후 시작 해 모든 Start() 이후
        yield return new WaitForSecondsRealtime(0.2f);
        ApplySavedStatesInScene();
    }

    private void ApplySavedStatesInScene()
    {
        var data = SaveManager.CurrentData;
        if (data == null) return;

        // === PASS 1: 위치 복원 (모든 UniqueId) ===
        var allUids = FindObjectsOfType<UniqueId>(true);
        foreach (var uid in allUids)
        {
            var go = uid.gameObject;
            if (SaveManager.TryGetObjectPosition(uid.Id, out var pos))
            {
                if (go.TryGetComponent<Rigidbody2D>(out var rb)) rb.position = pos;
                else go.transform.position = pos;
            }
        }

        // === PASS 2: 타입별 세부 상태 복원 ===
        foreach (var p in FindObjectsOfType<BasePossessable>(true))
        {
            if (!p.TryGetComponent(out UniqueId uid)) continue;
            if (SaveManager.TryGetPossessableState(uid.Id, out var has))
                p.ApplyHasActivatedFromSave(has);
        }

        foreach (var m in FindObjectsOfType<MemoryFragment>(true))
        {
            if (!m.TryGetComponent(out UniqueId uid)) continue;

            if (SaveManager.TryGetMemoryFragmentScannable(uid.Id, out bool scannable))
            {
                m.ApplyFromSave(scannable);
            }
            else if (SaveManager.HasCollectedMemoryID(m.data.memoryID))
            {
                m.ApplyFromSave(false); // 이미 수집된 건 스캔 불가
            }
        }

        foreach (var door in FindObjectsOfType<BaseDoor>(true))
        {
            if (door.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetDoorLocked(uid.Id, out bool locked))
            {
                door.SetLockedFromSave(locked);
            }
        }

        // === PASS 3: 활성/비활성 복원 (모든 UniqueId, 부모 -> 자식) ===
        var uidList = new List<UniqueId>(allUids);
        uidList.Sort((a, b) => GetDepth(a.transform).CompareTo(GetDepth(b.transform))); // 부모 먼저

        int savedActiveCount = SaveManager.CurrentData?.activeObjectStates?.Count ?? 0;
        int applied = 0;
        foreach (var uid in uidList)
        {
            if (SaveManager.TryGetActiveState(uid.Id, out bool activeGO))
            {
                uid.gameObject.SetActive(activeGO);
                applied++;
            }
        }
        Debug.Log($"[SaveStateApplier] Active states applied: {applied}/{savedActiveCount}");

        // === 인벤토리/진행도 ===
        var inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        SaveManager.ApplyPlayerInventoryFromSave(inv);
        MemoryManager.Instance?.WarmStartFromSave();
        ChapterEndingManager.Instance?.ApplyFromSave();

        // 로컬 함수: 트랜스폼 깊이(루트=0)
        static int GetDepth(Transform t)
        {
            int d = 0;
            while (t.parent != null) { d++; t = t.parent; }
            return d;
        }
    }
}
