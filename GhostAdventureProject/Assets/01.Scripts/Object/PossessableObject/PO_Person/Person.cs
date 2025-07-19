using UnityEngine;


// 컨디션 * UI * 를 관리하는 스크립트입니다.

public enum PersonCondition
{
    Vital,   // 활력
    Normal,  // 보통
    Tired    // 피곤함
}

public class Person : MonoBehaviour
{
    public PersonCondition currentCondition;
    private PersonCondition lastCondition;
    [SerializeField] private float yPos_UI = 2.5f; // UI의 y포지션 (오브젝트마다 알맞게 설정해주세요)
    public GameObject UI;
    public GameObject vitalUI; // 활력UI
    public GameObject normalUI; // 보통UI
    public GameObject tiredUI; // 피곤UI

    void Start()
    {   
        currentCondition = PersonCondition.Vital;
        ShowConditionUI();
    }
    
    void Update()
    {
        // UI는 컨디션이 바뀔때만 갱신
        if (currentCondition != lastCondition)
            {
                ShowConditionUI();
                lastCondition = currentCondition;
            }

        // UI 위치는 매 프레임 갱신 (움직이는 캐릭터일 경우)
        if (UI != null)
            UI.transform.position = transform.position + Vector3.up * yPos_UI;
    }    

    void ShowConditionUI()
    {
        vitalUI.SetActive(false);
        normalUI.SetActive(false);
        tiredUI.SetActive(false);

        // 현재 상태에 맞는 UI를 선택하고 위치 설정 + 켜기
        if (currentCondition == PersonCondition.Vital)
            UI = vitalUI;
        else if (currentCondition == PersonCondition.Normal)
            UI = normalUI;
        else if (currentCondition == PersonCondition.Tired)
            UI = tiredUI;

        Vector3 uiPos = transform.position + Vector3.up * yPos_UI;
        UI.transform.position = uiPos;

        UI.SetActive(true);
    }
}



