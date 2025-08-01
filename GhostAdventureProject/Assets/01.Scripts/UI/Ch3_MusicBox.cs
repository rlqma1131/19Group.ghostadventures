using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Ch3_MusicBox : BaseInteractable
{
    
    private bool playAble; // 오르골을 플레이 할 수 있는 영역에 있는지 확인
    private bool playMusicBox; // 오르골이 플레이되고 있는지 확인
    [SerializeField] GameObject QTEUI4;

    private void Update()
    {
        if(!playAble) return;

        if(Input.GetKeyDown(KeyCode.E))
        {
            StartQTE_MusicBox();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //SetHighlight(true);
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
    }
    
    void StartQTE_MusicBox()
    {
        QTEUI4.SetActive(true);
    }


}


