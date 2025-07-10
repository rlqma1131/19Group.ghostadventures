using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform targetToMove; // 움직일 대상 (InventoryPanel 전체)
    private Vector2 offset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetToMove == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetToMove,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetToMove == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetToMove.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            targetToMove.anchoredPosition = localPoint - offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료 시 필요한 작업
    }
}
