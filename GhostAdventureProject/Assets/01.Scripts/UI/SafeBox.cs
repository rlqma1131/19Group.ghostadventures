using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SafeBox : BaseInteractable
{   
    [SerializeField] private GameObject closeSafeBox; // 닫힌 금고
    [SerializeField] private GameObject openSafeBox; // 열린 금고
    [SerializeField] private ItemData needItem; // 금고를 여는데 필요한 아이템
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private GameObject ZoomSafeBox;
    [SerializeField] private GameObject q_Key;
    public bool safeBoxOpenAble; // 금고를 오픈할 수 있는 범위에 있는지 확인
    public bool safeBoxOpen; // 금고를 열었는지 확인
    Inventory_PossessableObject inventory;

    void Start()
    {
        safeBoxOpenAble = false;
        safeBoxOpen = false;
        ZoomSafeBox.SetActive(false);
        q_Key.SetActive(false);
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Q))
        {   
            if(safeBoxOpenAble && !safeBoxOpen)
            {
                inventory = Inventory_PossessableObject.Instance;
                
                if(inventory == null)
                {
                    UIManager.Instance.PromptUI.ShowPrompt("금고를 열 수 없습니다", 1.5f);
                    return;
                }
                if(needItem == inventory.selectedItem() && needItem != null && safeBoxOpenAble)
                {
                    UIManager.Instance.PromptUI.ShowPrompt("금고를 열었습니다.", 1.5f);
                    StartCoroutine(OpenSafeBox());
                    return;
                }
                return;
            }
        }
        
        if(safeBoxOpenAble && !safeBoxOpen)
            q_Key.SetActive(true);
        if(!safeBoxOpenAble)
            q_Key.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            if(!safeBoxOpen)
            {
                highlight.SetActive(true);
                safeBoxOpenAble = true;
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        safeBoxOpenAble = false;    
    }

    IEnumerator OpenSafeBox()
    {
        safeBoxOpen = true;
        highlight.SetActive(false);
        q_Key.SetActive(false);
        openSafeBox.SetActive(true);
        inventory.TryUseSelectedItem();
        ZoomSafeBox.SetActive(true);

        yield return new WaitForSeconds(1f);
        
        zoomCamera.Priority = 20; // ZoomSafeBox로 카메라 변경
        Debug.Log("금고 문 열림");
    }
}
