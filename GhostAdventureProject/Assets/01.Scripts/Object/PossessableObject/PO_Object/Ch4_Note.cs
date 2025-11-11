using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Ch4_Note : BaseInteractable
{
    [Header("Pattern (최초 1회 알림용)")]
    [SerializeField] Ch4_PatternAsset pattern;
    [SerializeField] bool revealOnce = true;

    [Header("Zoom UI (그 자리에서 켜고/끄기)")]
    [SerializeField] GameObject zoomRoot;

    bool isPlayerInside;
    bool isVisible;
    bool revealed;

    Player pl;
    Ch4_SpiderPuzzleController controller;

    protected override void Start()
    {
        base.Start();
        controller = FindObjectOfType<Ch4_SpiderPuzzleController>(true);
        pl = GameManager.Instance.Player;

        if (zoomRoot) zoomRoot.SetActive(false);
        isVisible = false;
    }

    void Update()
    {
        if (!isPlayerInside) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isVisible) Hide();
            else Show();
        }
    }

    void Show()
    {
        isVisible = true;
        if (zoomRoot) zoomRoot.SetActive(true);

        // 최초 1회만 패턴 확인 이벤트 전달
        if ((!revealed || !revealOnce) && controller)
        {
            controller.OnFirstNoteRevealed(pattern);
            revealed = true;
        }

        // 중복 입력 방지를 위해 잠시 인터랙트 목록에서 제거
        if (pl) pl.InteractSystem.RemoveInteractable(gameObject);
    }

    void Hide()
    {
        isVisible = false;
        if (zoomRoot) zoomRoot.SetActive(false);

        // 닫히고 아직 플레이어가 근처면 다시 상호작용 가능 목록에 복귀
        if (isPlayerInside && pl)
            pl.InteractSystem.AddInteractable(gameObject);
    }

    // 근접/이탈 시 상호작용 목록 관리
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
        if (!isVisible && pl)
            pl.InteractSystem.AddInteractable(gameObject);
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;

        // 범위를 벗어나면 자동으로 닫고, 목록에서도 제거
        if (isVisible) Hide();
        if (pl) pl.InteractSystem.RemoveInteractable(gameObject);
    }
}