// // MemoryStorageScroll.cs
// using DG.Tweening;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;

// public class MemoryStorageScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
// {
//     [Header("Scroll References")]
//     public ScrollRect scrollRect;
//     public RectTransform content;
//     public List<MemoryNode> memoryNodes = new();

//     [Header("Center & Snapping")]
//     public RectTransform centerReference;
//     public float snapDuration = 0.3f;
//     private int currentIndex = 0;

//     [Header("UI")]
//     public Text infoText;

//     private bool isDragging = false;

//     public void Initialize(List<MemoryData> memories)
//     {
//         // 노드 초기화
//         foreach (Transform child in content)
//             Destroy(child.gameObject);

//         memoryNodes.Clear();

//         for (int i = 0; i < memories.Count; i++)
//         {
//             GameObject nodeObj = Instantiate(Resources.Load<GameObject>("MemoryNode"), content);
//             MemoryNode node = nodeObj.GetComponent<MemoryNode>();
//             node.Initialize(memories[i]);
//             memoryNodes.Add(node);
//         }

//         // 첫 위치로 초기화
//         ScrollToIndex(0);
//     }

//     public void ScrollToIndex(int index)
//     {
//         if (index < 0 || index >= memoryNodes.Count) return;
//         currentIndex = index;

//         RectTransform target = memoryNodes[index].GetComponent<RectTransform>();
//         float difference = centerReference.localPosition.x - target.localPosition.x;

//         Vector3 newPos = content.localPosition + new Vector3(difference, 0f, 0f);
//         content.DOLocalMoveX(newPos.x, snapDuration).SetEase(Ease.OutCubic);

//         UpdateVisuals();
//     }

//     public void OnClickNext()
//     {
//         if (currentIndex < memoryNodes.Count - 1)
//             ScrollToIndex(currentIndex + 1);
//     }

//     public void OnClickPrev()
//     {
//         if (currentIndex > 0)
//             ScrollToIndex(currentIndex - 1);
//     }

//     private void UpdateVisuals()
//     {
//         for (int i = 0; i < memoryNodes.Count; i++)
//         {
//             float scale = (i == currentIndex) ? 1.1f : 0.8f;
//             float alpha = (i == currentIndex) ? 1f : 0.4f;

//             memoryNodes[i].transform.DOScale(scale, 0.2f);

//             var cg = memoryNodes[i].GetComponent<CanvasGroup>();
//             if (cg != null)
//                 cg.DOFade(alpha, 0.2f);
//         }

//         infoText.text = memoryNodes[currentIndex].memory.memoryTitle;
//     }

//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         isDragging = true;
//     }

//     public void OnEndDrag(PointerEventData eventData)
//     {
//         isDragging = false;
//         SnapToClosest();
//     }

//     private void SnapToClosest()
//     {
//         float closestDistance = float.MaxValue;
//         int closestIndex = currentIndex;

//         for (int i = 0; i < memoryNodes.Count; i++)
//         {
//             float dist = Mathf.Abs(memoryNodes[i].GetComponent<RectTransform>().position.x - centerReference.position.x);
//             if (dist < closestDistance)
//             {
//                 closestDistance = dist;
//                 closestIndex = i;
//             }
//         }

//         ScrollToIndex(closestIndex);
//     }
// }
