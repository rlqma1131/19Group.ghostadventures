using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.Playables;

public class Ch03_realmemorycutscene : MonoBehaviour
{
    [SerializeField] Ch3_MemoryPuzzleUI ch3_MemoryPuzzleUI;
    [SerializeField] PlayableDirector cutsceneDirector;

    bool isCutsceneActive;
    Player player;

    void Start() {
        player = GameManager.Instance.Player;
        cutsceneDirector.stopped += OnCutsceneStopped;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") && !isCutsceneActive && ch3_MemoryPuzzleUI.puzzlecompleted) {
            isCutsceneActive = true;
            player.PossessionSystem.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
            cutsceneDirector.Play();
            EnemyAI.PauseAllEnemies();
        }
    }

    void OnCutsceneStopped(PlayableDirector d) {
        EnemyAI.ResumeAllEnemies();
        player.PossessionSystem.CanMove = true;
        UIManager.Instance.PlayModeUI_OpenAll();
        isCutsceneActive = true;
    }
}