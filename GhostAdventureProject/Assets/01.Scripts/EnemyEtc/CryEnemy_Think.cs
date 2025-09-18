using UnityEngine;
using UnityEngine.UI;

// 울보의 말풍선UI를 관리하는 스크립트입니다.
public class CryEnemy_Think : MonoBehaviour
{
    [SerializeField] private Transform targetWorldObject; // 말풍선 띄울 대상
    [SerializeField] private Camera worldCamera;
    private RectTransform uiElement;     // 말풍선 UI 오브젝트
    public Image itemIcon;

    void Start()
    {
        uiElement = GetComponent<RectTransform>();

    }

    void Update()
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, targetWorldObject.position);
        uiElement.position = screenPos;
        Vector2 offset = new Vector2(0, 2); // 위로 2px만큼
    }
}
