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
        yield return null;
        ApplySavedStatesInScene();
    }

    private void ApplySavedStatesInScene()
    {
        Debug.Log($"SaveStateApplier : CurrentData가 존재하느냐? {SaveManager.CurrentData}");
        var data = SaveManager.CurrentData;
        if (data == null) return;

        // BasePossessable 위치 적용
        foreach (var p in FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid))
            {
                if (SaveManager.TryGetObjectPosition(uid.Id, out var pos))
                {
                    if (p.TryGetComponent<Rigidbody2D>(out var rb))
                        rb.position = pos;
                    else
                        p.transform.position = pos;
                }

                if (SaveManager.TryGetPossessableState(uid.Id, out var has))
                {
                    Debug.Log($"SaveStateApplier : {uid}, 위치 : {has}");
                    p.ApplyHasActivatedFromSave(has);
                }
            }
        }

        // MemoryFragment 스캔 가능/불가 적용
        foreach (var m in FindObjectsOfType<MemoryFragment>(true))
        {
            if (!m.TryGetComponent(out UniqueId uid)) continue;

            // 1) 저장에 명시적으로 있으면 그 값 사용
            if (SaveManager.TryGetMemoryFragmentScannable(uid.Id, out bool scannable))
            {
                m.ApplyFromSave(scannable);
                continue;
            }
            // 2) 이미 수집된 MemoryData면 스캔 불가
            if (SaveManager.HasCollectedMemoryID(m.data.memoryID))
            {
                m.ApplyFromSave(false);
            }
            // 3) 그 외엔 인스펙터 기본값 유지
        }

        // MemoryFragment 위치 적용
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

        // 문 잠금 상태 적용
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

        // === 진행도 복원 ===
        // 1) MemoryManager 먼저 (ID -> MemoryData 매핑 확보)
        MemoryManager.Instance?.WarmStartFromSave();

        // 2) CEM에 저장값 주입 → Notify()로 UI(PuzzleStatus) 자동 갱신
        ChapterEndingManager.Instance?.ApplyFromSave();
    }
}
