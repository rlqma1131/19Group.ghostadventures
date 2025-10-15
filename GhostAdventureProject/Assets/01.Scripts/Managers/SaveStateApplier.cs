using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.Object.BaseClasses.Interfaces;
using _01.Scripts.Object.NormalObject;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SaveStateApplier : Singleton<SaveStateApplier>
{
    void OnEnable() {
        SaveManager.Loaded += OnSaveLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SaveManager.CurrentData != null && gameObject.activeInHierarchy)
            StartCoroutine(ApplyNextFrame());
    }

    void OnDisable() {
        SaveManager.Loaded -= OnSaveLoaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSaveLoaded(SaveData _) {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ApplyNextFrame()); // Start 이후에 적용하기 위해 다음 프레임에
        else
            ApplySavedStatesInScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Additive) return; // 컷씬이면 적용 안함
        if (SaveManager.CurrentData != null) {
            StartCoroutine(ApplyNextFrame()); // 다음 프레임에 적용
        }
    }

    IEnumerator ApplyNextFrame() {
        // 한 프레임 이후 시작 해 모든 Start() 이후
        yield return new WaitForSecondsRealtime(0.2f);
        ApplySavedStatesInScene();
    }

    void ApplySavedStatesInScene() {
        SaveData data = SaveManager.CurrentData;
        if (data == null) return;

        Debug.Log("[SaveStateApplier]: 모든 Scene Object 상태 복원 시작");
        
        // === PASS 1: 모든 UniqueId를 가지고 있는 Object의 상태 복원 ===
        var allUids = new List<UniqueId>(FindObjectsOfType<UniqueId>(true));
        allUids.Sort((a, b) => GetDepth(a.transform).CompareTo(GetDepth(b.transform))); // 부모 먼저
        
        foreach (UniqueId uid in allUids) {
            GameObject go = uid.gameObject;
            if (go.TryGetComponent(out BasePossessable possessable)) {
                if (!SaveManager.TryGetPossessableObjectState(uid.Id, out PossessableObjectState possessableState))
                    continue;
                possessable.gameObject.SetActive(possessableState.active);
                possessable.SetScannable(possessableState.isScannable);
                possessable.transform.position = possessableState.position.ToVector3();
                possessable.SetActivated(possessableState.hasActivated);
            }
            else if (go.TryGetComponent(out MemoryFragment memoryFragment)) {
                if (!SaveManager.TryGetMemoryFragmentObjectState(uid.Id, out MemoryFragmentObjectState memoryState))
                    continue;
                memoryFragment.gameObject.SetActive(memoryState.active);
                memoryFragment.transform.position = memoryState.position.ToVector3();
                memoryFragment.SetScannable(memoryState.isScannable);
                memoryFragment.SetAlreadyScanned(memoryState.alreadyScanned);
            }
            else if (go.TryGetComponent(out BaseDoor door)) {
                if (SaveManager.TryGetDoorLocked(uid.Id, out bool locked))
                    door.SetLockedFromSave(locked);
            }
            else if (go.TryGetComponent(out Ch2_DrawingClue clue)) {
                if (!SaveManager.TryGetPossessableObjectState(uid.Id, out PossessableObjectState possessableState)) continue;
                clue.gameObject.SetActive(possessableState.active);
                clue.transform.position = possessableState.position.ToVector3();
                clue.ApplyHasActivatedFromSave(possessableState.hasActivated);
            }
            else if (go.TryGetComponent(out Ch4_Picture picture)) {
                if (!SaveManager.TryGetMemoryFragmentObjectState(uid.Id, out MemoryFragmentObjectState pictureState)) continue;
                picture.gameObject.SetActive(pictureState.active);
                picture.transform.position = pictureState.position.ToVector3();
                picture.SetPictureState(pictureState.isScannable, pictureState.alreadyScanned);
            }
            else {
                if (!SaveManager.TryGetObjectState(uid.Id, out ObjectState state)) continue;
                if (go.TryGetComponent(out IInteractable interactable)) interactable.SetScannable(state.isScannable);
                go.SetActive(state.active);
                go.transform.position = state.position.ToVector3();
            }
        }

        Debug.Log($"[SaveStateApplier] Active states applied: /{SaveManager.CurrentData.objectStateDict.Count + SaveManager.CurrentData.memoryFragmentObjectStateDict.Count + SaveManager.CurrentData.possessableObjectStateDict.Count}");

        // === PASS 2: Animator 복원 (부모 UniqueId 기준으로 자식 Animator 전부 적용)
        var reassertList = new List<(Animator anim, AnimatorChildSnapshot snap)>();

        foreach (UniqueId uid in allUids) {
            if (!SaveManager.TryGetAnimatorTree(uid.Id, out AnimatorTreeSnapshot tree) || tree?.animators == null) continue;

            Transform root = uid.transform;

            foreach (AnimatorChildSnapshot child in tree.animators) {
                Transform t = SaveManager.FindChildByPath(root, child.childPath);
                if (t == null) {
                    Debug.LogWarning($"[APPLY] path not found: uid={uid.Id} '{child.childPath}'");
                    continue;
                }

                Animator anim = t.GetComponent<Animator>();
                if (anim == null) {
                    Debug.LogWarning($"[APPLY] Animator missing at path: uid={uid.Id} '{child.childPath}'");
                    continue;
                }

                bool prevEnabled = anim.enabled;
                float prevSpeed = anim.speed;
                anim.enabled = false;

                // 남은 트리거 제거
                foreach (AnimatorControllerParameter p in anim.parameters) {
                    if (p.type == AnimatorControllerParameterType.Trigger)
                        anim.ResetTrigger(p.name);
                }

                // 내부 상태 초기화 + 강평가
                anim.Rebind();
                anim.Update(0f);

                // 레이어별 상태/진행도 적용
                foreach (AnimatorChildLayerState ly in child.layers) {
                    int l = ly.layer;
                    if (l >= anim.layerCount) continue;

                    float tNorm = ly.normalizedTime;
                    if (!float.IsFinite(tNorm)) tNorm = 0f;
                    tNorm = Mathf.Repeat(tNorm, 1f);
                    if (tNorm <= 0f) tNorm = 0.001f;
                    if (tNorm >= 0.999f) tNorm = 0.999f;

                    bool ok = TryPlay(anim, ly.fullPathHash, l, tNorm) || TryCrossFade(anim, ly.fullPathHash, l, tNorm);
                    anim.Update(0f);

                    AnimatorStateInfo cur = anim.GetCurrentAnimatorStateInfo(l);
                    if (cur.fullPathHash != ly.fullPathHash && ly.shortNameHash != 0 &&
                        cur.shortNameHash != ly.shortNameHash) {
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
        static bool TryPlay(Animator a, int hash, int layer, float t) {
            if (hash == 0) return false;
            try {
                a.Play(hash, layer, t);
                return true;
            }
            catch {
                return false;
            }
        }

        static bool TryCrossFade(Animator a, int hash, int layer, float t) {
            if (hash == 0) return false;
            try {
                a.CrossFade(hash, 0f, layer, t);
                return true;
            }
            catch {
                return false;
            }
        }

        // === 인벤토리/진행도 복원 ===
        Inventory_Player inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        SaveManager.ApplyPlayerInventoryFromSave(inv);
        MemoryManager.Instance?.WarmStartFromSave();
        ChapterEndingManager.Instance?.ApplyFromSave();

        // === 이벤트 완료 상태 복원 ===
        if (SaveManager.CurrentData?.eventCompletionDict != null) {
            foreach (var state in SaveManager.CurrentData.eventCompletionDict) {
                EventManager.Instance?.ApplyEventCompletedFromSave(state.Key, state.Value);
            }
        }

        // === 튜토리얼 완료 단계 복원 ===
        if (SaveManager.TryGetCompletedTutorialSteps(out var steps))
            TutorialManager.Instance?.ApplyFromSave(steps);

        // 로컬 함수: 트랜스폼 깊이(루트=0)
        static int GetDepth(Transform t) {
            int d = 0;
            while (t.parent != null) {
                d++;
                t = t.parent;
            }

            return d;
        }
        
        Debug.Log("[SaveStateApplier]: 모든 Scene Object 상태 복원 종료");
    }

    IEnumerator ReassertNextFrameForChildren(List<(Animator anim, AnimatorChildSnapshot snap)> list) {
        yield return null; // 모든 Start/OnEnable 이후 프레임

        foreach ((Animator anim, AnimatorChildSnapshot child) in list) {
            if (anim == null || child == null) continue;
            foreach (AnimatorChildLayerState ly in child.layers) {
                int l = ly.layer;
                if (l >= anim.layerCount) continue;

                float tNorm = ly.normalizedTime;
                if (!float.IsFinite(tNorm)) tNorm = 0f;
                tNorm = Mathf.Repeat(tNorm, 1f);
                if (tNorm <= 0f) tNorm = 0.001f;
                if (tNorm >= 0.999f) tNorm = 0.999f;

                AnimatorStateInfo cur = anim.GetCurrentAnimatorStateInfo(l);
                if (cur.fullPathHash != ly.fullPathHash &&
                    (ly.shortNameHash == 0 || cur.shortNameHash != ly.shortNameHash)) {
                    anim.Play(ly.fullPathHash != 0 ? ly.fullPathHash : ly.shortNameHash, l, tNorm);
                    anim.Update(0f);
                }
            }
        }
    }
}