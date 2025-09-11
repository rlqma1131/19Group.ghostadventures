using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UpDownStairs : MonoBehaviour
{
    [SerializeField] private Transform teleportPos;
    [SerializeField] private GameObject stairIcon;

    void Start()
    {
        stairIcon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GetComponent<BoxCollider2D>() != null) // 내가 BoxCollider일 때
        {
            stairIcon.SetActive(true);
        }
        if (GetComponent<PolygonCollider2D>() != null) // 내가 PolygonCollider일 때
        {
            Vector2 Pos = collision.transform.position;
            Pos.y = teleportPos.position.y;

            collision.transform.position = Pos;
            stairIcon.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GetComponent<BoxCollider2D>() != null) // BoxCollider에서 벗어났을 때만
        {
            stairIcon.SetActive(false);
        }
    }
}
