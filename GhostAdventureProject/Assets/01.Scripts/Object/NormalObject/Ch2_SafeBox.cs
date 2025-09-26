using System.Collections;
using Cinemachine;
using UnityEngine;
using System.Threading.Tasks;

public class Ch2_SafeBox : BaseInteractable
{   
    [SerializeField] private GameObject closeSafeBox;   // 닫힌 금고
    [SerializeField] private GameObject openSafeBox;    // 열린 금고
    [SerializeField] private ItemData   needItem;       // 금고를 여는데 필요한 아이템
    [SerializeField] private Ch2_SecurityGuard guard;   // 경비원
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private GameObject zoomSafeBox;
    [SerializeField] private GameObject q_Key;
    private Inventory_PossessableObject inventory;
    public bool safeBoxOpenAble;                        // 금고를 오픈할 수 있는 범위에 있는지 확인
    public bool safeBoxOpen;                            // 금고를 열었는지 확인


    protected override void Start()
    {
        base.Start();
        zoomSafeBox.SetActive(false);
        q_Key.SetActive(false);
        inventory = Inventory_PossessableObject.Instance;
        guard = FindObjectOfType<Ch2_SecurityGuard>();
    }

    void Update()
    {   
        if(!guard.HasActivated()) {
            safeBoxOpen = true;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {   
            if(safeBoxOpenAble && !safeBoxOpen)
            {
                if(needItem == null) return;
                if(needItem != inventory.selectedItem())
                {
                    UIManager.Instance.PromptUI.ShowPrompt("잠겨있어. 열쇠가 필요해");
                    return;
                }
                else if(needItem == inventory.selectedItem())
                {
                    StartCoroutine(OpenSafeBox());
                    return;
                }
                return;
            }
        }
    }
    
    IEnumerator OpenSafeBox()
    {
        safeBoxOpen = true;
        Highlight.SetActive(false);
        q_Key.SetActive(false);
        openSafeBox.SetActive(true);
        inventory.TryUseSelectedItem();
        zoomSafeBox.SetActive(true);

        yield return new WaitForSeconds(1f);
        
        zoomCamera.Priority = 20; // ZoomSafeBox로 카메라 변경
        player.PossessionSystem.CanMove = false;
    }

    public void ResetCamera()
    {
        zoomCamera.Priority = 5;
        zoomSafeBox.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            if(!safeBoxOpen)
            {
                Highlight.SetActive(true);
                safeBoxOpenAble = true;
                q_Key.SetActive(true);
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        safeBoxOpenAble = false;    
        q_Key.SetActive(false);
    }

}
