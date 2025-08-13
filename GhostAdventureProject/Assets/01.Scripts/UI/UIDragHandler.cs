using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform targetToMove; // 움직일 대상 (InventoryPanel 전체)
    private Vector2 offset;
    [SerializeField]private RectTransform parentRect;


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

            Vector2 newPos = localPoint - offset;


            Vector3[] parentCorners = new Vector3[4];
            Vector3[] targetCorners = new Vector3[4];

            parentRect.GetLocalCorners(parentCorners);
            targetToMove.GetLocalCorners(targetCorners);

            float halfWidth = (targetCorners[2].x - targetCorners[0].x) / 2f;
            float halfHeight = (targetCorners[2].y - targetCorners[0].y) / 2f;

            newPos.x = Mathf.Clamp(newPos.x, parentCorners[0].x + halfWidth, parentCorners[2].x - halfWidth);
            newPos.y = Mathf.Clamp(newPos.y, parentCorners[0].y + halfHeight, parentCorners[2].y - halfHeight);

            targetToMove.anchoredPosition = newPos;


        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료 시 필요한 작업
    }
}
