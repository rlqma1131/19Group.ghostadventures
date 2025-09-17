using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch03_EndMemory : MemoryFragment

//최종 퍼즐방 진입시 컷신 자동 재생+ 진행 저장
{
    bool activeCutscene = false;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activeCutscene)
        {
            Global_CutsceneManager.Instance.StartCoroutine(Global_CutsceneManager.Instance.PlayCutscene());
            activeCutscene = true;
            player.PossessionSystem.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
            EnemyAI.PauseAllEnemies();
            StartCoroutine(LoadNextSceneAfterDelay(3f));
        }
    }
    // 일정 시간(delay) 기다린 후 다음 씬 로드 + 진행 저장
    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Inventory_Player _inventory = GameManager.Instance.PlayerObj.GetComponent<Inventory_Player>();
        MemoryManager.Instance.TryCollect(data);
        SoundManager.Instance.FadeOutAndStopLoopingSFX();
        SceneManager.LoadScene("Ch03_End", LoadSceneMode.Additive);

        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);

        var chapter = DetectChapterFromScene(SceneManager.GetActiveScene().name);
        ChapterEndingManager.Instance.RegisterScannedMemory(data.memoryID, chapter);

        SaveManager.SaveWhenScanAfter(data.memoryID, data.memoryTitle,
            SceneManager.GetActiveScene().name,
            GameManager.Instance.PlayerObj.transform.position,
            checkpointId: data.memoryID,
            autosave: true);

        Debug.Log($"[MemoryFragment] 진행도 저장됨 : {data.memoryID} / {data.memoryTitle}");
    }
}
