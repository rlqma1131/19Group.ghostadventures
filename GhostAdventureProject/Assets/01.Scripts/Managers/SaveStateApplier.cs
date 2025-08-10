using System.Collections;
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
                SaveManager.TryGetPossessableState(uid.Id, out bool active))
            {
                p.SetActivatedFromSave(active);
                Debug.Log($"SaveStateApplier : {p}의 잠김 상태는 {active}로 적용됨");
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
                Debug.Log($"SaveStateApplier : {m}의 잠김 상태는 {scannable}로 적용됨");
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

        // 문
        foreach (var door in FindObjectsOfType<BaseDoor>(true))
        {
            if (door.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetDoorLocked(uid.Id, out bool locked))
            {
                door.SetLockedFromSave(locked);
                Debug.Log($"SaveStateApplier : {door}의 잠김 상태는 {locked}로 적용됨");
            }
        }

        // BasePossessable 활성/비활성 적용
        foreach (var p in FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetActiveState(uid.Id, out bool activeGO))
            {
                p.gameObject.SetActive(activeGO);
                // Debug.Log($"Apply Active (Possessable): {p.name} -> {activeGO}");
            }
        }

        // MemoryFragment 활성/비활성 적용
        foreach (var m in FindObjectsOfType<MemoryFragment>(true))
        {
            if (m.TryGetComponent(out UniqueId uid) &&
                SaveManager.TryGetActiveState(uid.Id, out bool activeGO))
            {
                m.gameObject.SetActive(activeGO);
                // Debug.Log($"Apply Active (MemoryFragment): {m.name} -> {activeGO}");
            }
        }
    }
}
