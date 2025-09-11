using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleTooltip : MonoBehaviour
{
    public GameObject tooltipImage; // 인스펙터에 툴팁 이미지 연결
    // public Sprite showSprite;  // 마우스 올렸을 때 표시할 이미지

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     tooltipImage.SetActive(true);
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     tooltipImage.SetActive(false);
    // }

    public void OnPointerEnter(PointerEventData e) { Debug.Log("ENTER " + name); }
    public void OnPointerExit(PointerEventData e)  { Debug.Log("EXIT " + name); }
}
