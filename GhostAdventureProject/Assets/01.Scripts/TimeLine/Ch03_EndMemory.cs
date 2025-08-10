using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch03_EndMemory : MonoBehaviour
{
    [SerializeField] private MemoryData memoryData;
    bool activeCutscene = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activeCutscene)
        {
            CutsceneManager.Instance.StartCoroutine(CutsceneManager.Instance.PlayCutscene());
            activeCutscene = true;
            PossessionSystem.Instance.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
            EnemyAI.PauseAllEnemies();
            StartCoroutine(LoadNextSceneAfterDelay(3f));
        }
    }

    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);



        Inventory_Player _inventory = GameManager.Instance.Player.GetComponent<Inventory_Player>();
        MemoryManager.Instance.TryCollect(memoryData);
        SoundManager.Instance.FadeOutAndStopLoopingSFX();
        SceneManager.LoadScene("Ch03_End", LoadSceneMode.Additive);
        ChapterEndingManager.Instance.RegisterScannedMemory(memoryData.memoryID, 3);
    }
}
