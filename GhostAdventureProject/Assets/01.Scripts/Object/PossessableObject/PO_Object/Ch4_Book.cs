using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch4_Book : BasePossessable
{
    [Header("Drop & Note")]
    [SerializeField] Transform dropPoint;            // 책이 떨어질 위치
    [SerializeField] GameObject note;          // 쪽지 프리팹(비활성 템플릿)

    bool hasDropped;

    protected override void Start() {
        base.Start();
        if(note) note.SetActive(false);
    }

    public override void TriggerEvent() {
        if (!IsPossessed || hasDropped) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.position = dropPoint
                ? dropPoint.position
                : transform.position + Vector3.down * 0.4f;

            // 쪽지 노출(상호작용 가능해짐)
            if (note) note.SetActive(true);
            hasActivated = false;
            player.InteractSystem.RemoveInteractable(gameObject);
            UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode("쪽지가 나타났다", 1.3f);
            hasDropped = true;
        }
    }
}
