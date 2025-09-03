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
        
        foreach (var clue in FindObjectsOfType<Ch2_DrawingClue>(true))
        {
            if (!clue.TryGetComponent(out UniqueId uid)) continue;
            if (SaveManager.TryGetPossessableState(uid.Id, out bool has))
                clue.ApplyHasActivatedFromSave(has); // <-- 저장된 값 복원
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

        // === PASS 4: Animator 복원 (부모 UniqueId 기준으로 자식 Animator 전부 적용)
        var reassertList = new System.Collections.Generic.List<(Animator anim, AnimatorChildSnapshot snap)>();

        foreach (var uid in allUids)
        {
            if (!SaveManager.TryGetAnimatorTree(uid.Id, out var tree) || tree?.animators == null) continue;

            var root = uid.transform;

            foreach (var child in tree.animators)
            {
                var t = SaveManager.FindChildByPath(root, child.childPath);
                if (t == null) { Debug.LogWarning($"[APPLY] path not found: uid={uid.Id} '{child.childPath}'"); continue; }

                var anim = t.GetComponent<Animator>();
                if (anim == null) { Debug.LogWarning($"[APPLY] Animator missing at path: uid={uid.Id} '{child.childPath}'"); continue; }

                bool prevEnabled = anim.enabled;
                float prevSpeed = anim.speed;
                anim.enabled = false;

                // 남은 트리거 제거
                foreach (var p in anim.parameters)
                    if (p.type == AnimatorControllerParameterType.Trigger)
                        anim.ResetTrigger(p.name);

                // 내부 상태 초기화 + 강평가
                anim.Rebind();
                anim.Update(0f);

                // 레이어별 상태/진행도 적용
                foreach (var ly in child.layers)
                {
                    int l = ly.layer;
                    if (l >= anim.layerCount) continue;

                    float tNorm = ly.normalizedTime;
                    if (!float.IsFinite(tNorm)) tNorm = 0f;
                    tNorm = Mathf.Repeat(tNorm, 1f);
                    if (tNorm <= 0f) tNorm = 0.001f;
                    if (tNorm >= 0.999f) tNorm = 0.999f;

                    bool ok = TryPlay(anim, ly.fullPathHash, l, tNorm) || TryCrossFade(anim, ly.fullPathHash, l, tNorm);
                    anim.Update(0f);

                    var cur = anim.GetCurrentAnimatorStateInfo(l);
                    if (cur.fullPathHash != ly.fullPathHash && ly.shortNameHash != 0 && cur.shortNameHash != ly.shortNameHash)
                    {
                        // full 실패시 짧은 해시나 0초 CrossFade로 마지막 보정
                        anim.CrossFade(ly.fullPathHash != 0 ? ly.fullPathHash : ly.shortNameHash, 0f, l, tNorm);
                        anim.Update(0f);
                    }
                }

                anim.enabled = prevEnabled;
                anim.speed = prevSpeed;

                // 다음 프레임에 한 번 더 보정(다른 초기화가 덮어써도 되돌림)
                reassertList.Add((anim, child));
            }
        }

        // 안전망: 다음 프레임에 재보정
        StartCoroutine(ReassertNextFrameForChildren(reassertList));

        // 헬퍼들 (지역 함수로 둬도 됨)
        static bool TryPlay(Animator a, int hash, int layer, float t)
        { if (hash == 0) return false; try { a.Play(hash, layer, t); return true; } catch { return false; } }
        static bool TryCrossFade(Animator a, int hash, int layer, float t)
        { if (hash == 0) return false; try { a.CrossFade(hash, 0f, layer, t); return true; } catch { return false; } }

        // === 인벤토리/진행도 ===
        var inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        SaveManager.ApplyPlayerInventoryFromSave(inv);
        MemoryManager.Instance?.WarmStartFromSave();
        ChapterEndingManager.Instance?.ApplyFromSave();

        // === 튜토리얼 완료 단계 복원 ===
        if (SaveManager.TryGetCompletedTutorialSteps(out var steps))
            TutorialManager.Instance?.ApplyFromSave(steps);

        // 로컬 함수: 트랜스폼 깊이(루트=0)
        static int GetDepth(Transform t)
        {
            int d = 0;
            while (t.parent != null) { d++; t = t.parent; }
            return d;
        }
    }

    private IEnumerator ReassertNextFrameForChildren(System.Collections.Generic.List<(Animator anim, AnimatorChildSnapshot snap)> list)
    {
        yield return null; // 모든 Start/OnEnable 이후 프레임

        foreach (var (anim, child) in list)
        {
            if (anim == null || child == null) continue;
            foreach (var ly in child.layers)
            {
                int l = ly.layer;
                if (l >= anim.layerCount) continue;

                float tNorm = ly.normalizedTime;
                if (!float.IsFinite(tNorm)) tNorm = 0f;
                tNorm = Mathf.Repeat(tNorm, 1f);
                if (tNorm <= 0f) tNorm = 0.001f;
                if (tNorm >= 0.999f) tNorm = 0.999f;

                var cur = anim.GetCurrentAnimatorStateInfo(l);
                if (cur.fullPathHash != ly.fullPathHash && (ly.shortNameHash == 0 || cur.shortNameHash != ly.shortNameHash))
                {
                    anim.Play(ly.fullPathHash != 0 ? ly.fullPathHash : ly.shortNameHash, l, tNorm);
                    anim.Update(0f);
                }
            }
        }
    }
}
